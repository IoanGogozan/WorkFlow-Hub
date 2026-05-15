using NorvixHub.Application.Audit;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Cases;
using NorvixHub.Domain.Cases;
using NorvixHub.Domain.Intake;

namespace NorvixHub.Api.Endpoints;

public static partial class CaseEndpoints
{
    private static CaseWorkspace CreateCaseFromIntake(IntakeItem intake, ITenantContext tenantContext)
    {
        return new CaseWorkspace
        {
            TenantId = intake.TenantId,
            CreatedBy = tenantContext.UserId,
            CaseNumber = $"CASE-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            Title = intake.Subject,
            Description = intake.Body,
            OwnerUserId = tenantContext.UserId,
            SourceIntakeItemId = intake.Id,
            MissingInformationJson = """["Confirm customer contact person","Attach relevant documentation"]"""
        };
    }

    private static CaseResponse ToResponse(CaseWorkspace caseWorkspace)
    {
        return new CaseResponse(
            caseWorkspace.Id,
            caseWorkspace.TenantId,
            caseWorkspace.CaseNumber,
            caseWorkspace.Title,
            caseWorkspace.Description,
            caseWorkspace.Status.ToString(),
            caseWorkspace.OwnerUserId,
            caseWorkspace.DueDate,
            caseWorkspace.SourceIntakeItemId,
            caseWorkspace.CreatedAt);
    }

    private static CaseTaskResponse ToResponse(CaseTask task)
    {
        return new CaseTaskResponse(
            task.Id,
            task.CaseId,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.DueDate,
            task.CreatedAt);
    }

    private static CaseNoteResponse ToResponse(CaseNote note)
    {
        return new CaseNoteResponse(
            note.Id,
            note.CaseId,
            note.Body,
            note.Visibility,
            note.CreatedAt);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static Task WriteAuditAsync(
        IAuditEventWriter auditEventWriter,
        CaseWorkspace caseWorkspace,
        ITenantContext tenantContext,
        HttpContext httpContext,
        string action,
        CancellationToken cancellationToken)
    {
        var request = new AuditEventRequest(
            caseWorkspace.TenantId,
            tenantContext.UserId,
            "User",
            "Case",
            caseWorkspace.Id.ToString(),
            action,
            null,
            $$"""{"status":"{{caseWorkspace.Status}}"}""",
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            httpContext.TraceIdentifier);

        return auditEventWriter.WriteAsync(request, cancellationToken);
    }
}

