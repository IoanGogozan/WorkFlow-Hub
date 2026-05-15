using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Integrations;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Integrations;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class IntegrationEndpoints
{
    private static async Task<IResult> SyncIntegration(
        string provider,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IIntegrationSyncAdapter adapter,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageIntegrations(tenantContext) || !IsKnownProvider(provider))
        {
            return !IsKnownProvider(provider) ? Results.NotFound() : Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var connection = await FindConnectionAsync(provider, tenantContext, dbContext, cancellationToken);
        if (connection is null || connection.Status == IntegrationConnectionStatus.Disconnected)
        {
            return Results.BadRequest(new { error = "Integration must be connected before sync." });
        }

        var response = await RunSyncAsync(connection, adapter, dbContext, tenantContext, "Manual", null, cancellationToken);
        await WriteAuditAsync(auditEventWriter, connection, tenantContext, httpContext, "IntegrationSyncRunCreated", cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> ListSyncRuns(
        string provider,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!IsKnownProvider(provider))
        {
            return Results.NotFound();
        }

        var normalizedProvider = NormalizeProvider(provider);
        var runs = await dbContext.IntegrationSyncRuns
            .Where(run => run.TenantId == tenantContext.TenantId && run.Provider == normalizedProvider)
            .OrderByDescending(run => run.StartedAt)
            .Take(50)
            .Select(run => ToResponse(run))
            .ToListAsync(cancellationToken);

        return Results.Ok(runs);
    }

    private static async Task<IResult> RetrySyncRun(
        string provider,
        Guid syncRunId,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IIntegrationSyncAdapter adapter,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageIntegrations(tenantContext) || !IsKnownProvider(provider))
        {
            return !IsKnownProvider(provider) ? Results.NotFound() : Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var normalizedProvider = NormalizeProvider(provider);
        var originalRun = await dbContext.IntegrationSyncRuns.SingleOrDefaultAsync(
            run => run.Id == syncRunId && run.TenantId == tenantContext.TenantId && run.Provider == normalizedProvider,
            cancellationToken);
        if (originalRun is null || originalRun.Status != IntegrationSyncStatus.Failed)
        {
            return Results.NotFound();
        }

        var connection = await FindConnectionAsync(provider, tenantContext, dbContext, cancellationToken);
        if (connection is null || connection.Status == IntegrationConnectionStatus.Disconnected)
        {
            return Results.BadRequest(new { error = "Integration must be connected before retry." });
        }

        var response = await RunSyncAsync(connection, adapter, dbContext, tenantContext, "Retry", originalRun.Id, cancellationToken);
        await WriteAuditAsync(auditEventWriter, connection, tenantContext, httpContext, "IntegrationSyncRetried", cancellationToken);
        return Results.Ok(response);
    }

    private static Task<int> CountFailedSyncsAsync(
        IntegrationConnection connection,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return dbContext.IntegrationSyncRuns.CountAsync(
            run =>
                run.TenantId == connection.TenantId &&
                run.ConnectionId == connection.Id &&
                run.Status == IntegrationSyncStatus.Failed,
            cancellationToken);
    }
}
