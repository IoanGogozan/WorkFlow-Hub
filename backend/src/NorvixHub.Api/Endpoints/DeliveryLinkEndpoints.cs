using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Delivery;
using NorvixHub.Domain.Delivery;
using NorvixHub.Domain.Documents;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DeliveryEndpoints
{
    private static async Task<IResult> GeneratePdf(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IFileStorage fileStorage,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageDelivery(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var package = await FindPackageAsync(id, tenantContext, dbContext, cancellationToken);
        if (package is null)
        {
            return Results.NotFound();
        }

        var caseTitle = await dbContext.Cases
            .Where(caseWorkspace => caseWorkspace.Id == package.CaseId && caseWorkspace.TenantId == package.TenantId)
            .Select(caseWorkspace => caseWorkspace.Title)
            .SingleAsync(cancellationToken);
        var documentTitles = await dbContext.DeliveryPackageItems
            .Where(item => item.TenantId == package.TenantId && item.DeliveryPackageId == package.Id)
            .OrderBy(item => item.DisplayName)
            .Select(item => item.DisplayName)
            .ToListAsync(cancellationToken);
        var pdfBytes = CreatePdfSummaryBytes(package.Title, caseTitle, documentTitles);
        await using var pdfStream = new MemoryStream(pdfBytes);
        var filename = $"{SanitizeFilename(package.Title)}-summary.pdf";
        var stored = await fileStorage.SaveAsync(
            pdfStream,
            filename,
            "application/pdf",
            cancellationToken);

        var summaryDocument = new DocumentRecord
        {
            TenantId = package.TenantId,
            CreatedBy = tenantContext.UserId,
            Title = filename
        };
        var version = new DocumentVersion
        {
            TenantId = summaryDocument.TenantId,
            CreatedBy = tenantContext.UserId,
            DocumentId = summaryDocument.Id,
            VersionNumber = 1,
            BlobContainer = stored.Container,
            BlobName = stored.BlobName,
            OriginalFilename = filename,
            ContentType = "application/pdf",
            SizeBytes = stored.SizeBytes,
            Sha256Hash = stored.Sha256Hash,
            UploadedByUserId = tenantContext.UserId
        };
        summaryDocument.LinkToCase(package.CaseId, tenantContext.UserId, DateTimeOffset.UtcNow);
        summaryDocument.SetCurrentVersion(version.Id, tenantContext.UserId, DateTimeOffset.UtcNow);
        package.MarkSummaryGenerated(summaryDocument.Id, tenantContext.UserId, DateTimeOffset.UtcNow);
        dbContext.Documents.Add(summaryDocument);
        dbContext.DocumentVersions.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, package, tenantContext, httpContext, "DeliveryPdfGenerated", cancellationToken);
        return Results.Ok(await ToResponseAsync(package, dbContext, null, null, cancellationToken));
    }

    private static async Task<IResult> CreateLink(
        Guid id,
        CreateDeliveryLinkRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageDelivery(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var package = await FindPackageAsync(id, tenantContext, dbContext, cancellationToken);
        if (package is null)
        {
            return Results.NotFound();
        }

        if (request.ExpiresAt <= DateTimeOffset.UtcNow || request.ExpiresAt > DateTimeOffset.UtcNow.AddDays(30))
        {
            return Results.BadRequest(new { error = "Expiry must be in the future and no more than 30 days out." });
        }

        var token = CreateToken();
        var link = new DeliveryLink
        {
            TenantId = package.TenantId,
            CreatedBy = tenantContext.UserId,
            DeliveryPackageId = package.Id,
            TokenHash = HashToken(token),
            RecipientEmail = request.RecipientEmail,
            ExpiresAt = request.ExpiresAt
        };
        package.MarkDelivered(tenantContext.UserId, DateTimeOffset.UtcNow);
        dbContext.DeliveryLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, package, tenantContext, httpContext, "DeliveryLinkCreated", cancellationToken);
        return Results.Ok(await ToResponseAsync(package, dbContext, token, link.Id, cancellationToken));
    }

    private static async Task<IResult> RevokeLink(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageDelivery(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var link = await dbContext.DeliveryLinks.SingleOrDefaultAsync(
            candidate => candidate.Id == id && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);
        if (link is null)
        {
            return Results.NotFound();
        }

        link.Revoke(tenantContext.UserId, DateTimeOffset.UtcNow);
        var package = await dbContext.DeliveryPackages.FindAsync(new object?[] { link.DeliveryPackageId }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        if (package is not null)
        {
            await WriteAuditAsync(auditEventWriter, package, tenantContext, httpContext, "DeliveryLinkRevoked", cancellationToken);
        }

        return Results.NoContent();
    }
}
