using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Delivery;
using NorvixHub.Domain.Delivery;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DeliveryEndpoints
{
    private static bool CanManageDelivery(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or TenantRole.Admin or TenantRole.OperationsUser;
    }

    private static async Task<DeliveryPackageResponse?> ToResponseAsync(
        DeliveryPackage package,
        NorvixHubDbContext dbContext,
        string? token,
        CancellationToken cancellationToken)
    {
        var items = await dbContext.DeliveryPackageItems
            .Where(item => item.TenantId == package.TenantId && item.DeliveryPackageId == package.Id)
            .OrderBy(item => item.DisplayName)
            .Select(item => new DeliveryPackageItemResponse(item.Id, item.DocumentId, item.DisplayName))
            .ToListAsync(cancellationToken);

        var links = await dbContext.DeliveryLinks
            .Where(link => link.TenantId == package.TenantId && link.DeliveryPackageId == package.Id)
            .OrderByDescending(link => link.CreatedAt)
            .Select(link => new DeliveryLinkResponse(
                link.Id,
                link.ExpiresAt,
                link.RevokedAt,
                link.RecipientEmail,
                token))
            .ToListAsync(cancellationToken);

        return new DeliveryPackageResponse(
            package.Id,
            package.CaseId,
            package.Title,
            package.Status.ToString(),
            package.SummaryPdfDocumentId,
            package.SummaryGeneratedAt,
            items,
            links);
    }

    private static Task<DeliveryPackage?> FindPackageAsync(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return dbContext.DeliveryPackages.SingleOrDefaultAsync(
            package => package.Id == id && package.TenantId == tenantContext.TenantId,
            cancellationToken);
    }

    private static Task WriteAuditAsync(
        IAuditEventWriter auditEventWriter,
        DeliveryPackage package,
        ITenantContext tenantContext,
        HttpContext httpContext,
        string action,
        CancellationToken cancellationToken)
    {
        var request = new AuditEventRequest(
            package.TenantId,
            tenantContext.UserId,
            "User",
            "DeliveryPackage",
            package.Id.ToString(),
            action,
            null,
            $$"""{"status":"{{package.Status}}"}""",
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            httpContext.TraceIdentifier);

        return auditEventWriter.WriteAsync(request, cancellationToken);
    }
}
