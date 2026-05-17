using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Cases;
using NorvixHub.Domain.Cases;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class CaseEndpoints
{
    public static IEndpointRouteBuilder MapCaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cases");

        group.MapGet("/", ListCases).WithName("ListCases");
        group.MapGet("/{id:guid}", GetCase).WithName("GetCase");
        group.MapPost("/{id:guid}/tasks", AddTask).WithName("AddCaseTask");
        group.MapPost("/{id:guid}/notes", AddNote).WithName("AddCaseNote");
        group.MapGet("/{id:guid}/activity", GetActivity).WithName("GetCaseActivity");

        app.MapPost("/api/intakes/{id:guid}/convert-to-case", ConvertIntakeToCase)
            .WithName("ConvertIntakeToCase");

        return app;
    }

    private static async Task<IResult> ConvertIntakeToCase(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanWriteCases(tenantContext) || tenantContext.UserId is not { } userId)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var intake = await dbContext.IntakeItems.SingleOrDefaultAsync(
            candidate => candidate.Id == id && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);
        if (intake is null)
        {
            return Results.NotFound();
        }

        if (intake.ConvertedCaseId is { } existingCaseId)
        {
            var existing = await FindCaseAsync(existingCaseId, tenantContext, dbContext, cancellationToken);
            return existing is null ? Results.NotFound() : Results.Ok(ToResponse(existing));
        }

        if (intake.Status is IntakeStatus.Rejected or IntakeStatus.ConvertedToCase)
        {
            return Results.BadRequest(new { error = "Intake cannot be converted in its current status." });
        }

        var caseWorkspace = CreateCaseFromIntake(intake, tenantContext);
        dbContext.Cases.Add(caseWorkspace);
        intake.MarkConvertedToCase(caseWorkspace.Id, userId, DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(auditEventWriter, caseWorkspace, tenantContext, httpContext, "CaseCreated", cancellationToken);
        return Results.Created($"/api/cases/{caseWorkspace.Id}", ToResponse(caseWorkspace));
    }

    private static async Task<IResult> ListCases(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var cases = await dbContext.Cases
            .Where(caseWorkspace => caseWorkspace.TenantId == tenantId)
            .OrderByDescending(caseWorkspace => caseWorkspace.CreatedAt)
            .Select(caseWorkspace => new CaseListItemResponse(
                caseWorkspace.Id,
                caseWorkspace.CaseNumber,
                caseWorkspace.Title,
                caseWorkspace.Status.ToString(),
                caseWorkspace.OwnerUserId,
                caseWorkspace.DueDate,
                caseWorkspace.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(cases);
    }

    private static async Task<IResult> GetCase(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var caseWorkspace = await FindCaseAsync(id, tenantContext, dbContext, cancellationToken);
        return caseWorkspace is null ? Results.NotFound() : Results.Ok(ToResponse(caseWorkspace));
    }

    private static async Task<IResult> AddTask(
        Guid id,
        CreateCaseTaskRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanWriteCases(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var caseWorkspace = await FindCaseAsync(id, tenantContext, dbContext, cancellationToken);
        if (caseWorkspace is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 240)
        {
            return Results.BadRequest(new { error = "Task title is required and must be 240 characters or fewer." });
        }

        var task = new CaseTask
        {
            TenantId = caseWorkspace.TenantId,
            CreatedBy = tenantContext.UserId,
            CaseId = caseWorkspace.Id,
            Title = request.Title.Trim(),
            Description = NormalizeOptional(request.Description),
            DueDate = request.DueDate
        };

        dbContext.CaseTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, caseWorkspace, tenantContext, httpContext, "CaseTaskCreated", cancellationToken);
        return Results.Created($"/api/cases/{caseWorkspace.Id}/tasks/{task.Id}", ToResponse(task));
    }

    private static async Task<IResult> AddNote(
        Guid id,
        CreateCaseNoteRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanWriteCases(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var caseWorkspace = await FindCaseAsync(id, tenantContext, dbContext, cancellationToken);
        if (caseWorkspace is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Length > 4000)
        {
            return Results.BadRequest(new { error = "Note body is required and must be 4000 characters or fewer." });
        }

        var note = new CaseNote
        {
            TenantId = caseWorkspace.TenantId,
            CreatedBy = tenantContext.UserId,
            CaseId = caseWorkspace.Id,
            Body = request.Body.Trim(),
            Visibility = NormalizeOptional(request.Visibility) ?? "Internal"
        };

        dbContext.CaseNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, caseWorkspace, tenantContext, httpContext, "CaseNoteCreated", cancellationToken);
        return Results.Created($"/api/cases/{caseWorkspace.Id}/notes/{note.Id}", ToResponse(note));
    }

    private static async Task<IResult> GetActivity(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var caseWorkspace = await FindCaseAsync(id, tenantContext, dbContext, cancellationToken);
        if (caseWorkspace is null)
        {
            return Results.NotFound();
        }

        var entityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            caseWorkspace.Id.ToString()
        };

        if (caseWorkspace.SourceIntakeItemId is { } intakeId)
        {
            entityIds.Add(intakeId.ToString());
        }

        var documentIds = await dbContext.Documents
            .Where(document => document.TenantId == caseWorkspace.TenantId && document.CaseId == caseWorkspace.Id)
            .Select(document => document.Id)
            .ToListAsync(cancellationToken);
        foreach (var documentId in documentIds)
        {
            entityIds.Add(documentId.ToString());
        }

        var deliveryPackageIds = await dbContext.DeliveryPackages
            .Where(package => package.TenantId == caseWorkspace.TenantId && package.CaseId == caseWorkspace.Id)
            .Select(package => package.Id)
            .ToListAsync(cancellationToken);
        foreach (var packageId in deliveryPackageIds)
        {
            entityIds.Add(packageId.ToString());
        }

        var activity = await dbContext.AuditEvents
            .Where(audit => audit.TenantId == caseWorkspace.TenantId && entityIds.Contains(audit.EntityId))
            .Select(audit => new CaseActivityResponse(
                audit.Id,
                audit.EntityType,
                audit.EntityId,
                audit.Action,
                audit.ActorUserId,
                audit.CreatedAt))
            .ToListAsync(cancellationToken);

        if (deliveryPackageIds.Count > 0)
        {
            var publicAccessActivity = await dbContext.DeliveryAccessLogs
                .Where(access => access.TenantId == caseWorkspace.TenantId &&
                    deliveryPackageIds.Contains(access.DeliveryPackageId))
                .Select(access => new CaseActivityResponse(
                    access.Id,
                    "PublicDelivery",
                    access.DocumentId.HasValue
                        ? access.DocumentId.Value.ToString()
                        : access.DeliveryLinkId.ToString(),
                    access.Action,
                    null,
                    access.CreatedAt))
                .ToListAsync(cancellationToken);
            activity.AddRange(publicAccessActivity);
        }

        activity = activity
            .OrderByDescending(item => item.CreatedAt)
            .ToList();

        return Results.Ok(activity);
    }

    private static bool CanWriteCases(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or TenantRole.Admin or TenantRole.OperationsUser;
    }

    private static Task<CaseWorkspace?> FindCaseAsync(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return dbContext.Cases.SingleOrDefaultAsync(
            candidate => candidate.Id == id && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);
    }

}
