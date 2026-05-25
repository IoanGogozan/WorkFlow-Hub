using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.AI;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.AI;
using NorvixHub.Domain.AI;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.Reviews;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class AiReviewEndpoints
{
    public static IEndpointRouteBuilder MapAiReviewEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/intakes/{id:guid}/latest-ai", GetLatestIntakeAi).WithName("GetLatestIntakeAi");
        app.MapPost("/api/intakes/{id:guid}/analyze", AnalyzeIntake).WithName("AnalyzeIntake");
        app.MapPost("/api/intakes/{id:guid}/approve-ai", ApproveIntakeAi).WithName("ApproveIntakeAi");
        app.MapPost("/api/intakes/{id:guid}/reject-ai", RejectIntakeAi).WithName("RejectIntakeAi");
        return app;
    }

    private static async Task<IResult> GetLatestIntakeAi(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!CanReview(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var intake = await FindIntakeAsync(id, tenantContext, dbContext, cancellationToken);
        if (intake is null)
        {
            return Results.NotFound();
        }

        var run = await dbContext.AiAnalysisRuns
            .Where(candidate =>
                candidate.TenantId == intake.TenantId &&
                candidate.EntityType == "IntakeItem" &&
                candidate.EntityId == intake.Id)
            .OrderByDescending(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (run is null)
        {
            return Results.NotFound();
        }

        var suggestion = JsonSerializer.Deserialize<AiIntakeSuggestionResponse>(run.OutputJson);
        return suggestion is null ? Results.NotFound() : Results.Ok(ToResponse(run, suggestion));
    }

    private static async Task<IResult> AnalyzeIntake(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAiReviewProvider aiReviewProvider,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanReview(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var intake = await FindIntakeAsync(id, tenantContext, dbContext, cancellationToken);
        if (intake is null)
        {
            return Results.NotFound();
        }

        var inputHash = HashInput(intake);
        var existingRun = await dbContext.AiAnalysisRuns
            .Where(candidate =>
                candidate.TenantId == intake.TenantId &&
                candidate.EntityType == "IntakeItem" &&
                candidate.EntityId == intake.Id &&
                candidate.InputHash == inputHash &&
                candidate.Status == AiAnalysisStatus.NeedsReview)
            .OrderByDescending(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRun is not null)
        {
            var existingSuggestion = JsonSerializer.Deserialize<AiIntakeSuggestionResponse>(existingRun.OutputJson);
            if (existingSuggestion is not null)
            {
                return Results.Ok(ToResponse(existingRun, existingSuggestion));
            }
        }

        var suggestion = aiReviewProvider.AnalyzeIntake(intake);
        var outputJson = JsonSerializer.Serialize(ToResponse(suggestion));
        var run = CreateRun(intake, aiReviewProvider, suggestion, outputJson);
        var reviewTask = CreateReviewTask(intake, run, tenantContext);

        intake.MarkAiNeedsReview(tenantContext.UserId, DateTimeOffset.UtcNow);
        dbContext.AiAnalysisRuns.Add(run);
        dbContext.ReviewTasks.Add(reviewTask);
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(auditEventWriter, intake, tenantContext, httpContext, "AiAnalysisRequested", cancellationToken);
        return Results.Ok(ToResponse(run, suggestion));
    }

    private static async Task<IResult> ApproveIntakeAi(
        Guid id,
        ApproveAiSuggestionRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanReview(tenantContext) || tenantContext.UserId is not { } userId)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var reviewState = await FindReviewStateAsync(id, request.AiAnalysisRunId, tenantContext, dbContext, cancellationToken);
        if (reviewState is null)
        {
            return Results.NotFound();
        }

        var now = DateTimeOffset.UtcNow;
        reviewState.Intake.ApproveAiSuggestion(
            request.CustomerName,
            request.OrganizationNumber,
            request.Category,
            request.Urgency,
            userId,
            now);
        reviewState.Run.MarkApproved(userId, now);
        reviewState.Task.MarkApproved(userId, JsonSerializer.Serialize(request), now);

        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, reviewState.Intake, tenantContext, httpContext, "AiSuggestionApproved", cancellationToken);
        return Results.Ok(IntakeEndpoints.ToResponse(reviewState.Intake));
    }

    private static async Task<IResult> RejectIntakeAi(
        Guid id,
        ApproveAiSuggestionRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanReview(tenantContext) || tenantContext.UserId is not { } userId)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var reviewState = await FindReviewStateAsync(id, request.AiAnalysisRunId, tenantContext, dbContext, cancellationToken);
        if (reviewState is null)
        {
            return Results.NotFound();
        }

        var now = DateTimeOffset.UtcNow;
        reviewState.Run.MarkRejected(userId, now);
        reviewState.Task.MarkRejected(userId, JsonSerializer.Serialize(request), now);
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(auditEventWriter, reviewState.Intake, tenantContext, httpContext, "AiSuggestionRejected", cancellationToken);
        return Results.NoContent();
    }

    private static bool CanReview(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or
            TenantRole.Admin or
            TenantRole.OperationsUser or
            TenantRole.Reviewer;
    }

    private static Task<IntakeItem?> FindIntakeAsync(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return dbContext.IntakeItems
            .SingleOrDefaultAsync(intake => intake.Id == id && intake.TenantId == tenantContext.TenantId, cancellationToken);
    }

    private static async Task<ReviewState?> FindReviewStateAsync(
        Guid intakeId,
        Guid runId,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var intake = await FindIntakeAsync(intakeId, tenantContext, dbContext, cancellationToken);
        var run = await dbContext.AiAnalysisRuns.SingleOrDefaultAsync(
            candidate => candidate.Id == runId && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);
        var task = await dbContext.ReviewTasks.SingleOrDefaultAsync(
            candidate => candidate.AiAnalysisRunId == runId && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);

        return intake is null || run is null || task is null ? null : new ReviewState(intake, run, task);
    }

    private static AiAnalysisRun CreateRun(
        IntakeItem intake,
        IAiReviewProvider aiReviewProvider,
        AiIntakeSuggestion suggestion,
        string outputJson)
    {
        return new AiAnalysisRun
        {
            TenantId = intake.TenantId,
            EntityType = "IntakeItem",
            EntityId = intake.Id,
            Provider = aiReviewProvider.Provider,
            Model = aiReviewProvider.Model,
            PromptVersion = aiReviewProvider.PromptVersion,
            InputHash = HashInput(intake),
            OutputJson = outputJson,
            Confidence = suggestion.Confidence
        };
    }

    private static ReviewTask CreateReviewTask(IntakeItem intake, AiAnalysisRun run, ITenantContext tenantContext)
    {
        return new ReviewTask
        {
            TenantId = intake.TenantId,
            CreatedBy = tenantContext.UserId,
            EntityType = "IntakeItem",
            EntityId = intake.Id,
            ReviewType = "AiIntakeSuggestion",
            AiAnalysisRunId = run.Id
        };
    }

    private static string HashInput(IntakeItem intake)
    {
        var input = $"{intake.Id}|{intake.Subject}|{intake.Body}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }

    private static AiAnalysisRunResponse ToResponse(AiAnalysisRun run, AiIntakeSuggestion suggestion)
    {
        return new AiAnalysisRunResponse(
            run.Id,
            run.EntityId,
            run.EntityType,
            run.Provider,
            run.Model,
            run.PromptVersion,
            run.Confidence,
            run.Status.ToString(),
            ToResponse(suggestion),
            run.CreatedAt);
    }

    private static AiAnalysisRunResponse ToResponse(AiAnalysisRun run, AiIntakeSuggestionResponse suggestion)
    {
        return new AiAnalysisRunResponse(
            run.Id,
            run.EntityId,
            run.EntityType,
            run.Provider,
            run.Model,
            run.PromptVersion,
            run.Confidence,
            run.Status.ToString(),
            suggestion,
            run.CreatedAt);
    }

    private static AiIntakeSuggestionResponse ToResponse(AiIntakeSuggestion suggestion)
    {
        return new AiIntakeSuggestionResponse(
            suggestion.CustomerName,
            suggestion.OrganizationNumber,
            suggestion.Category,
            suggestion.Urgency,
            suggestion.SuggestedTasks,
            suggestion.Summary,
            suggestion.MissingInformation,
            suggestion.Confidence);
    }

    private static Task WriteAuditAsync(
        IAuditEventWriter auditEventWriter,
        IntakeItem intake,
        ITenantContext tenantContext,
        HttpContext httpContext,
        string action,
        CancellationToken cancellationToken)
    {
        var request = new AuditEventRequest(
            intake.TenantId,
            tenantContext.UserId,
            "User",
            "IntakeItem",
            intake.Id.ToString(),
            action,
            null,
            $$"""{"status":"{{intake.Status}}"}""",
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            httpContext.TraceIdentifier);

        return auditEventWriter.WriteAsync(request, cancellationToken);
    }

    private sealed record ReviewState(IntakeItem Intake, AiAnalysisRun Run, ReviewTask Task);
}
