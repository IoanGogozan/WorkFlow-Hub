using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Contracts.Documents;
using NorvixHub.Domain.Documents;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private static Task<DocumentRecord?> FindDocumentAsync(
        Guid id,
        Application.Tenancy.ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return dbContext.Documents.SingleOrDefaultAsync(
            candidate => candidate.Id == id && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);
    }

    private static DocumentResponse ToResponse(DocumentRecord document)
    {
        return new DocumentResponse(
            document.Id,
            document.TenantId,
            document.Title,
            document.Status.ToString(),
            document.DocumentType,
            document.CurrentVersionId,
            document.CaseId,
            document.ExpiryDate,
            document.CreatedAt);
    }

    private static DocumentVersionResponse ToResponse(DocumentVersion version)
    {
        return new DocumentVersionResponse(
            version.Id,
            version.DocumentId,
            version.VersionNumber,
            version.OriginalFilename,
            version.ContentType,
            version.SizeBytes,
            version.Sha256Hash,
            version.CreatedAt);
    }

    private static DocumentClassificationResponse ToClassificationResponse(
        Guid runId,
        Application.Documents.DocumentClassificationSuggestion suggestion)
    {
        return new DocumentClassificationResponse(
            runId,
            suggestion.DocumentType,
            suggestion.ExpiryDate,
            suggestion.Summary,
            suggestion.Confidence);
    }

    private static Task WriteAuditAsync(
        IAuditEventWriter auditEventWriter,
        DocumentRecord document,
        Application.Tenancy.ITenantContext tenantContext,
        HttpContext httpContext,
        string action,
        CancellationToken cancellationToken)
    {
        var request = new AuditEventRequest(
            document.TenantId,
            tenantContext.UserId,
            "User",
            "Document",
            document.Id.ToString(),
            action,
            null,
            $$"""{"status":"{{document.Status}}"}""",
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            httpContext.TraceIdentifier);

        return auditEventWriter.WriteAsync(request, cancellationToken);
    }
}
