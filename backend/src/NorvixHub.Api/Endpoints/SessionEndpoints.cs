using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Auth;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api");

        group.MapGet("/me", GetCurrentUser).WithName("GetCurrentUser");
        group.MapGet("/tenants", GetTenants).WithName("GetTenants");
        group.MapPost("/tenants/{tenantId:guid}/switch", SwitchTenant).WithName("SwitchTenant");

        return app;
    }

    private static async Task<IResult> GetCurrentUser(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.IsAuthenticated ||
            tenantContext.UserId is not { } userId ||
            tenantContext.TenantId is not { } tenantId ||
            tenantContext.Role is not { } role)
        {
            return Results.Unauthorized();
        }

        var user = await dbContext.Users
            .Where(candidate => candidate.Id == userId)
            .Select(candidate => new { candidate.DisplayName, candidate.Email })
            .SingleAsync(cancellationToken);

        return Results.Ok(new CurrentUserResponse(
            userId,
            tenantId,
            user.DisplayName,
            user.Email,
            role.ToString()));
    }

    private static async Task<IResult> GetTenants(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.IsAuthenticated || tenantContext.UserId is not { } userId)
        {
            return Results.Unauthorized();
        }

        var memberships = await dbContext.TenantMemberships
            .Where(membership => membership.UserId == userId)
            .ToListAsync(cancellationToken);

        var tenantIds = memberships.Select(membership => membership.TenantId).ToArray();
        var tenantsById = await dbContext.Tenants
            .Where(tenant => tenantIds.Contains(tenant.Id))
            .ToDictionaryAsync(tenant => tenant.Id, cancellationToken);

        var tenants = memberships
            .Select(membership =>
            {
                var tenant = tenantsById[membership.TenantId];
                return new TenantSummaryResponse(
                    tenant.Id,
                    tenant.Name,
                    tenant.Slug,
                    membership.Role.ToString());
            })
            .OrderBy(tenant => tenant.Name)
            .ToList();

        return Results.Ok(tenants);
    }

    private static async Task<IResult> SwitchTenant(
        Guid tenantId,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.IsAuthenticated || tenantContext.UserId is not { } userId)
        {
            return Results.Unauthorized();
        }

        var hasMembership = await dbContext.TenantMemberships
            .AnyAsync(
                membership => membership.UserId == userId && membership.TenantId == tenantId,
                cancellationToken);

        return hasMembership ? Results.NoContent() : Results.StatusCode(StatusCodes.Status403Forbidden);
    }
}
