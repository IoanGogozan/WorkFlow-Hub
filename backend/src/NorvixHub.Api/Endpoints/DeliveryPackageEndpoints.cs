using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Delivery;
using NorvixHub.Domain.Delivery;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DeliveryEndpoints
{
    private static async Task<IResult> CreatePackage(
        Guid caseId,
        CreateDeliveryPackageRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageDelivery(tenantContext) || request.DocumentIds.Count == 0)
        {
            return request.DocumentIds.Count == 0 ? Results.BadRequest(new { error = "At least one document is required." }) : Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var caseTitle = await dbContext.Cases
            .Where(caseWorkspace => caseWorkspace.Id == caseId && caseWorkspace.TenantId == tenantContext.TenantId)
            .Select(caseWorkspace => caseWorkspace.Title)
            .SingleOrDefaultAsync(cancellationToken);
        if (caseTitle is null)
        {
            return Results.NotFound();
        }

        var documents = await dbContext.Documents
            .Where(document => request.DocumentIds.Contains(document.Id) &&
                document.TenantId == tenantContext.TenantId &&
                document.CaseId == caseId)
            .ToListAsync(cancellationToken);
        if (documents.Count != request.DocumentIds.Distinct().Count())
        {
            return Results.NotFound();
        }

        var package = new DeliveryPackage
        {
            TenantId = tenantContext.TenantId!.Value,
            CreatedBy = tenantContext.UserId,
            CaseId = caseId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? $"Delivery package - {caseTitle}" : request.Title.Trim()
        };
        dbContext.DeliveryPackages.Add(package);
        dbContext.DeliveryPackageItems.AddRange(documents.Select(document => new DeliveryPackageItem
        {
            TenantId = package.TenantId,
            CreatedBy = tenantContext.UserId,
            DeliveryPackageId = package.Id,
            DocumentId = document.Id,
            DisplayName = document.Title
        }));

        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, package, tenantContext, httpContext, "DeliveryPackageCreated", cancellationToken);
        return Results.Created($"/api/delivery-packages/{package.Id}", await ToResponseAsync(package, dbContext, null, null, cancellationToken));
    }

    private static async Task<IResult> GetPackage(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var package = await FindPackageAsync(id, tenantContext, dbContext, cancellationToken);
        return package is null
            ? Results.NotFound()
            : Results.Ok(await ToResponseAsync(package, dbContext, null, null, cancellationToken));
    }
}
