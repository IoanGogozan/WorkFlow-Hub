using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Auth;

public sealed class LocalDevAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        NorvixHubDbContext dbContext,
        LocalDevTenantContext tenantContext)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
        {
            await next(httpContext);
            return;
        }

        if (!TryReadGuid(httpContext, "X-Norvix-User-Id", out var userId) ||
            !TryReadGuid(httpContext, "X-Norvix-Tenant-Id", out var tenantId))
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(new { error = "Local dev auth headers are required." });
            return;
        }

        var membership = await dbContext.TenantMemberships
            .Where(candidate => candidate.UserId == userId && candidate.TenantId == tenantId)
            .Select(candidate => new { candidate.Role })
            .SingleOrDefaultAsync(httpContext.RequestAborted);

        var userIsActive = await dbContext.Users
            .Where(user => user.Id == userId)
            .Select(user => user.IsActive)
            .SingleOrDefaultAsync(httpContext.RequestAborted);

        if (membership is null || !userIsActive)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { error = "No active membership for tenant." });
            return;
        }

        tenantContext.SetAuthenticated(tenantId, userId, membership.Role);
        await next(httpContext);
    }

    private static bool TryReadGuid(HttpContext httpContext, string headerName, out Guid value)
    {
        value = Guid.Empty;
        return httpContext.Request.Headers.TryGetValue(headerName, out var rawValue) &&
            Guid.TryParse(rawValue, out value);
    }
}

