using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Documents;
using NorvixHub.Domain.AI;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Reviews;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private static async Task<IResult> ListDocuments(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var documents = await dbContext.Documents
            .Where(document => document.TenantId == tenantId)
            .OrderByDescending(document => document.CreatedAt)
            .Select(document => ToResponse(document))
            .ToListAsync(cancellationToken);

        return Results.Ok(documents);
    }

    private static async Task<IResult> GetDocument(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var document = await FindDocumentAsync(id, tenantContext, dbContext, cancellationToken);
        return document is null ? Results.NotFound() : Results.Ok(ToResponse(document));
    }

    private static async Task<IResult> LinkToCase(
        Guid id,
        LinkDocumentToCaseRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanWriteDocuments(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var document = await FindDocumentAsync(id, tenantContext, dbContext, cancellationToken);
        var caseExists = await dbContext.Cases.AnyAsync(
            caseWorkspace => caseWorkspace.Id == request.CaseId && caseWorkspace.TenantId == tenantContext.TenantId,
            cancellationToken);
        if (document is null || !caseExists)
        {
            return Results.NotFound();
        }

        document.LinkToCase(request.CaseId, tenantContext.UserId, DateTimeOffset.UtcNow);
        dbContext.DocumentLinks.Add(CreateCaseLink(document, request.CaseId, tenantContext));
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, document, tenantContext, httpContext, "DocumentLinkedToCase", cancellationToken);
        return Results.Ok(ToResponse(document));
    }

    private static async Task<IResult> AnalyzeDocument(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IDocumentClassificationProvider classifier,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanReviewDocuments(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var document = await FindDocumentAsync(id, tenantContext, dbContext, cancellationToken);
        var version = await FindCurrentVersionAsync(document, dbContext, cancellationToken);
        if (document is null || version is null)
        {
            return Results.NotFound();
        }

        var suggestion = classifier.Classify(document, version);
        var response = ToClassificationResponse(Guid.NewGuid(), suggestion);
        var run = CreateAnalysisRun(document, classifier, suggestion, response);
        var reviewTask = CreateReviewTask(document, run, tenantContext);
        document.MarkNeedsReview(tenantContext.UserId, DateTimeOffset.UtcNow);

        dbContext.AiAnalysisRuns.Add(run);
        dbContext.ReviewTasks.Add(reviewTask);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, document, tenantContext, httpContext, "DocumentClassificationRequested", cancellationToken);

        return Results.Ok(ToClassificationResponse(run.Id, suggestion));
    }

    private static async Task<IResult> ApproveClassification(
        Guid id,
        ApproveDocumentClassificationRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanReviewDocuments(tenantContext) || tenantContext.UserId is not { } userId)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (string.IsNullOrWhiteSpace(request.DocumentType) || request.DocumentType.Length > 120)
        {
            return Results.BadRequest(new { error = "Document type is required and must be 120 characters or fewer." });
        }

        var state = await FindReviewStateAsync(id, request.AiAnalysisRunId, tenantContext, dbContext, cancellationToken);
        if (state is null)
        {
            return Results.NotFound();
        }

        var now = DateTimeOffset.UtcNow;
        state.Document.ApproveClassification(request.DocumentType, request.ExpiryDate, userId, now);
        state.Run.MarkApproved(userId, now);
        state.Task.MarkApproved(userId, JsonSerializer.Serialize(request), now);

        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, state.Document, tenantContext, httpContext, "DocumentClassificationApproved", cancellationToken);
        return Results.Ok(ToResponse(state.Document));
    }
}
