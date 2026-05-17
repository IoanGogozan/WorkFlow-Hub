using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Demo;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Auth;

public sealed class LocalDevAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        NorvixHubDbContext dbContext,
        LocalDevTenantContext tenantContext,
        IWebHostEnvironment environment)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
        {
            await next(httpContext);
            return;
        }

        if (httpContext.Request.Path.Equals("/api/demo-sessions", StringComparison.OrdinalIgnoreCase))
        {
            await next(httpContext);
            return;
        }

        if (TryReadBearerToken(httpContext, out var token))
        {
            if (await TryAuthenticateDemoSessionAsync(token, httpContext, dbContext, tenantContext))
            {
                await next(httpContext);
                return;
            }

            return;
        }

        if (!environment.IsDevelopment())
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(new { error = "Demo session bearer token is required." });
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

    private static async Task<bool> TryAuthenticateDemoSessionAsync(
        string token,
        HttpContext httpContext,
        NorvixHubDbContext dbContext,
        LocalDevTenantContext tenantContext)
    {
        var tokenHash = DemoToken.Hash(token);
        var session = await dbContext.DemoSessions
            .Where(candidate => candidate.TokenHash == tokenHash)
            .SingleOrDefaultAsync(httpContext.RequestAborted);

        if (session is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(new { error = "Invalid demo session token." });
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        if (!session.IsActive(now))
        {
            if (session.Status == DemoSessionStatus.Active)
            {
                session.MarkExpired();
                await dbContext.SaveChangesAsync(httpContext.RequestAborted);
            }

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(new { error = "Demo session expired." });
            return false;
        }

        var membership = await dbContext.TenantMemberships
            .Where(candidate => candidate.UserId == session.UserId && candidate.TenantId == session.TenantId)
            .Select(candidate => new { candidate.Role })
            .SingleOrDefaultAsync(httpContext.RequestAborted);
        var userIsActive = await dbContext.Users
            .Where(user => user.Id == session.UserId)
            .Select(user => user.IsActive)
            .SingleOrDefaultAsync(httpContext.RequestAborted);

        if (membership is null || !userIsActive)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { error = "No active membership for demo tenant." });
            return false;
        }

        session.MarkSeen(now);
        await dbContext.SaveChangesAsync(httpContext.RequestAborted);
        tenantContext.SetAuthenticated(session.TenantId, session.UserId, membership.Role);
        return true;
    }

    private static bool TryReadBearerToken(HttpContext httpContext, out string token)
    {
        const string prefix = "Bearer ";
        token = string.Empty;
        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var rawValue))
        {
            return false;
        }

        var authorization = rawValue.ToString();
        if (!authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = authorization[prefix.Length..].Trim();
        return token.Length > 0;
    }

    private static bool TryReadGuid(HttpContext httpContext, string headerName, out Guid value)
    {
        value = Guid.Empty;
        return httpContext.Request.Headers.TryGetValue(headerName, out var rawValue) &&
            Guid.TryParse(rawValue, out value);
    }
}
