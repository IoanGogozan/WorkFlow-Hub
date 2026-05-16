using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private static async Task<IResult> DownloadDocument(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        var document = await FindDocumentAsync(id, tenantContext, dbContext, cancellationToken);
        var version = await FindCurrentVersionAsync(document, dbContext, cancellationToken);
        if (document is null || version is null)
        {
            return Results.NotFound();
        }

        var stored = await fileStorage.OpenReadAsync(
            version.BlobContainer,
            version.BlobName,
            cancellationToken);
        if (stored is null)
        {
            return Results.NotFound();
        }

        return Results.File(
            stored.Content,
            version.ContentType,
            version.OriginalFilename,
            enableRangeProcessing: true);
    }
}
