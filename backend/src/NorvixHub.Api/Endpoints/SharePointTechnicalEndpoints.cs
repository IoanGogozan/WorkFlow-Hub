using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.SharePoint;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class SharePointTechnicalEndpoints
{
    public static IEndpointRouteBuilder MapSharePointTechnicalEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/technical/sharepoint/status", GetStatus);
        app.MapGet("/api/technical/sharepoint/tree", GetTree);
        app.MapGet("/api/technical/sharepoint/documents", GetDocuments);
        app.MapGet("/api/technical/sharepoint/operations", GetOperations);
        app.MapPost("/api/technical/sharepoint/test-restricted-access", TestRestrictedAccess);
        return app;
    }

    private static bool Allowed(ITenantContext context) =>
        context.TenantId is not null && context.Role is TenantRole.TenantOwner or TenantRole.Admin;

    private static IResult GetStatus(ITenantContext context, ISharePointDocumentAdapterResolver resolver) =>
        Allowed(context) ? Results.Ok(resolver.GetCurrent().GetStatus()) : Results.StatusCode(403);

    private static async Task<IResult> GetTree(
        ITenantContext context,
        NorvixHubDbContext db,
        CancellationToken token)
    {
        if (!Allowed(context))
        {
            return Results.StatusCode(403);
        }

        var paths = await db.SimulatedSharePointOperations
            .AsNoTracking()
            .Where(operation => operation.TenantId == context.TenantId && operation.Operation == "CreateFolder")
            .Select(operation => operation.Target)
            .Distinct()
            .OrderBy(path => path)
            .ToListAsync(token);
        return Results.Ok(paths);
    }

    private static async Task<IResult> GetDocuments(
        ITenantContext context,
        NorvixHubDbContext db,
        CancellationToken token)
    {
        if (!Allowed(context))
        {
            return Results.StatusCode(403);
        }

        var documents = await db.SimulatedSharePointDocumentItems
            .AsNoTracking()
            .Where(item => item.TenantId == context.TenantId)
            .OrderByDescending(item => item.LastSyncedAt)
            .Take(100)
            .Select(item => new
            {
                item.Name,
                item.ParentPath,
                item.ExternalItemId,
                item.ETag,
                item.Version,
                item.SyncStatus,
                item.LastSyncedAt
            })
            .ToListAsync(token);
        return Results.Ok(documents);
    }

    private static async Task<IResult> GetOperations(
        ITenantContext context,
        NorvixHubDbContext db,
        CancellationToken token)
    {
        if (!Allowed(context))
        {
            return Results.StatusCode(403);
        }

        var operations = await db.SimulatedSharePointOperations
            .AsNoTracking()
            .Where(operation => operation.TenantId == context.TenantId)
            .OrderByDescending(operation => operation.CreatedAt)
            .Take(100)
            .Select(operation => new
            {
                operation.CreatedAt,
                operation.HttpMethod,
                operation.Operation,
                operation.Target,
                operation.StatusCode,
                operation.Succeeded,
                operation.DurationMilliseconds,
                operation.ErrorCode
            })
            .ToListAsync(token);
        return Results.Ok(operations);
    }
    private static async Task<IResult> TestRestrictedAccess(ITenantContext context, ISharePointDocumentAdapterResolver resolver, CancellationToken token) =>
        !Allowed(context) ? Results.StatusCode(403) : Results.Ok(await resolver.GetCurrent().TestSiteAccessAsync(context.TenantId!.Value, "site-hr-internal", token));
}
