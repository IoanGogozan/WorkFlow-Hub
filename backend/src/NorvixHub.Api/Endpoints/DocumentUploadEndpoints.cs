using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private static async Task<IResult> UploadDocument(
        HttpRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IFileStorage fileStorage,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanWriteDocuments(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var upload = await ReadUploadAsync(request, cancellationToken);
        if (!upload.IsValid)
        {
            return Results.BadRequest(new { error = upload.Error });
        }

        await using var stream = upload.File!.OpenReadStream();
        var stored = await fileStorage.SaveAsync(
            stream,
            upload.File.FileName,
            upload.File.ContentType,
            cancellationToken);

        var document = CreateDocument(upload, tenantContext);
        var version = CreateVersion(document, stored, upload.File, 1, tenantContext);
        document.SetCurrentVersion(version.Id, tenantContext.UserId, DateTimeOffset.UtcNow);

        dbContext.Documents.Add(document);
        dbContext.DocumentVersions.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, document, tenantContext, httpContext, "DocumentUploaded", cancellationToken);

        return Results.Created($"/api/documents/{document.Id}", ToResponse(document));
    }

    private static async Task<IResult> UploadVersion(
        Guid id,
        HttpRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IFileStorage fileStorage,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanWriteDocuments(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var document = await FindDocumentAsync(id, tenantContext, dbContext, cancellationToken);
        if (document is null)
        {
            return Results.NotFound();
        }

        var upload = await ReadUploadAsync(request, cancellationToken);
        if (!upload.IsValid)
        {
            return Results.BadRequest(new { error = upload.Error });
        }

        var nextVersion = await dbContext.DocumentVersions
            .Where(version => version.TenantId == document.TenantId && version.DocumentId == document.Id)
            .MaxAsync(version => (int?)version.VersionNumber, cancellationToken) + 1 ?? 1;
        await using var stream = upload.File!.OpenReadStream();
        var stored = await fileStorage.SaveAsync(stream, upload.File.FileName, upload.File.ContentType, cancellationToken);
        var version = CreateVersion(document, stored, upload.File, nextVersion, tenantContext);

        document.SetCurrentVersion(version.Id, tenantContext.UserId, DateTimeOffset.UtcNow);
        dbContext.DocumentVersions.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, document, tenantContext, httpContext, "DocumentVersionUploaded", cancellationToken);

        return Results.Created($"/api/documents/{document.Id}/versions/{version.Id}", ToResponse(version));
    }
}
