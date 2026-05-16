using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Documents;
using NorvixHub.Contracts.Delivery;
using NorvixHub.Domain.Delivery;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DeliveryEndpoints
{
    private static async Task<IResult> OpenDelivery(
        string token,
        NorvixHubDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var state = await FindPublicStateAsync(token, dbContext, cancellationToken);
        if (state is null)
        {
            return Results.NotFound();
        }

        if (!state.Link.IsActive(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status410Gone);
        }

        await LogAccessAsync(state, null, "ViewedPackage", dbContext, httpContext, cancellationToken);
        var documents = await GetPublicDocumentsAsync(state, dbContext, cancellationToken);
        return Results.Ok(new PublicDeliveryPackageResponse(
            state.Package.Title,
            state.CaseTitle,
            state.Link.ExpiresAt,
            documents));
    }

    private static async Task<IResult> OpenDeliveryDocument(
        string token,
        Guid documentId,
        NorvixHubDbContext dbContext,
        IFileStorage fileStorage,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var state = await FindPublicStateAsync(token, dbContext, cancellationToken);
        if (state is null)
        {
            return Results.NotFound();
        }

        if (!state.Link.IsActive(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status410Gone);
        }

        var itemExists = await dbContext.DeliveryPackageItems.AnyAsync(
            item => item.TenantId == state.Package.TenantId &&
                item.DeliveryPackageId == state.Package.Id &&
                item.DocumentId == documentId,
            cancellationToken);
        if (!itemExists)
        {
            return Results.NotFound();
        }

        await LogAccessAsync(state, documentId, "ViewedDocument", dbContext, httpContext, cancellationToken);
        var document = await dbContext.Documents.SingleAsync(
            candidate => candidate.Id == documentId && candidate.TenantId == state.Package.TenantId,
            cancellationToken);
        if (document.CurrentVersionId is not { } versionId)
        {
            return Results.NotFound();
        }

        var version = await dbContext.DocumentVersions.SingleOrDefaultAsync(
            candidate => candidate.Id == versionId && candidate.TenantId == state.Package.TenantId,
            cancellationToken);
        if (version is null)
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

    private static async Task<PublicDeliveryState?> FindPublicStateAsync(
        string token,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(token);
        var link = await dbContext.DeliveryLinks.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == tokenHash,
            cancellationToken);
        if (link is null)
        {
            return null;
        }

        var package = await dbContext.DeliveryPackages.SingleAsync(
            candidate => candidate.Id == link.DeliveryPackageId && candidate.TenantId == link.TenantId,
            cancellationToken);
        var caseTitle = await dbContext.Cases
            .Where(caseWorkspace => caseWorkspace.Id == package.CaseId && caseWorkspace.TenantId == package.TenantId)
            .Select(caseWorkspace => caseWorkspace.Title)
            .SingleAsync(cancellationToken);
        return new PublicDeliveryState(link, package, caseTitle);
    }

    private static async Task<List<PublicDeliveryDocumentResponse>> GetPublicDocumentsAsync(
        PublicDeliveryState state,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var documentIds = await dbContext.DeliveryPackageItems
            .Where(item => item.TenantId == state.Package.TenantId && item.DeliveryPackageId == state.Package.Id)
            .Select(item => item.DocumentId)
            .ToListAsync(cancellationToken);
        return await dbContext.Documents
            .Where(document => document.TenantId == state.Package.TenantId && documentIds.Contains(document.Id))
            .OrderBy(document => document.Title)
            .Select(document => new PublicDeliveryDocumentResponse(document.Id, document.Title, document.DocumentType))
            .ToListAsync(cancellationToken);
    }

    private static async Task LogAccessAsync(
        PublicDeliveryState state,
        Guid? documentId,
        string action,
        NorvixHubDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        dbContext.DeliveryAccessLogs.Add(new DeliveryAccessLog
        {
            TenantId = state.Package.TenantId,
            DeliveryLinkId = state.Link.Id,
            DeliveryPackageId = state.Package.Id,
            DocumentId = documentId,
            Action = action,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record PublicDeliveryState(DeliveryLink Link, DeliveryPackage Package, string CaseTitle);
}
