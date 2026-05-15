using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
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

        var summaryDocument = new DocumentRecord
        {
            TenantId = package.TenantId,
            CreatedBy = tenantContext.UserId,
            Title = $"{package.Title} summary.pdf"
        };
        summaryDocument.LinkToCase(package.CaseId, tenantContext.UserId, DateTimeOffset.UtcNow);
        package.MarkSummaryGenerated(summaryDocument.Id, tenantContext.UserId, DateTimeOffset.UtcNow);
        dbContext.Documents.Add(summaryDocument);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, package, tenantContext, httpContext, "DeliveryPdfGenerated", cancellationToken);
        return Results.Ok(await ToResponseAsync(package, dbContext, null, cancellationToken));
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
        return Results.Ok(await ToResponseAsync(package, dbContext, token, cancellationToken));
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
