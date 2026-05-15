using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Integrations;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Integrations;
using NorvixHub.Domain.Integrations;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class IntegrationEndpoints
{
    private static readonly Dictionary<string, string> KnownProviders = new()
    {
        ["brreg"] = "Brønnøysundregistrene",
        ["microsoft-graph"] = "Microsoft Graph / SharePoint",
        ["tripletex"] = "Tripletex Accounting",
        ["powerbi-fabric"] = "Power BI / Fabric"
    };

    private static bool CanManageIntegrations(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or TenantRole.Admin;
    }

    private static bool IsKnownProvider(string provider)
    {
        return KnownProviders.ContainsKey(NormalizeProvider(provider));
    }

    private static string NormalizeProvider(string provider)
    {
        return provider.Trim().ToLowerInvariant();
    }

    private static async Task<IntegrationConnection> GetOrCreateConnectionAsync(
        string provider,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var normalizedProvider = NormalizeProvider(provider);
        var connection = await FindConnectionAsync(normalizedProvider, tenantContext, dbContext, cancellationToken);
        if (connection is not null)
        {
            return connection;
        }

        connection = new IntegrationConnection
        {
            TenantId = tenantContext.TenantId!.Value,
            CreatedBy = tenantContext.UserId,
            Provider = normalizedProvider,
            DisplayName = KnownProviders[normalizedProvider]
        };
        dbContext.IntegrationConnections.Add(connection);
        return connection;
    }

    private static Task<IntegrationConnection?> FindConnectionAsync(
        string provider,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var normalizedProvider = NormalizeProvider(provider);
        return dbContext.IntegrationConnections.SingleOrDefaultAsync(
            connection => connection.Provider == normalizedProvider && connection.TenantId == tenantContext.TenantId,
            cancellationToken);
    }

    private static async Task<IntegrationSyncRunResponse> RunSyncAsync(
        IntegrationConnection connection,
        IIntegrationSyncAdapter adapter,
        NorvixHubDbContext dbContext,
        ITenantContext tenantContext,
        string triggeredBy,
        Guid? retriedFromRunId,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var run = new IntegrationSyncRun
        {
            TenantId = connection.TenantId,
            CreatedBy = tenantContext.UserId,
            ConnectionId = connection.Id,
            Provider = connection.Provider,
            TriggeredBy = triggeredBy,
            RetriedFromSyncRunId = retriedFromRunId,
            StartedAt = startedAt
        };

        var result = await adapter.SyncAsync(connection, cancellationToken);
        var status = result.Succeeded ? IntegrationSyncStatus.Succeeded : IntegrationSyncStatus.Failed;
        var completedAt = DateTimeOffset.UtcNow;
        run.Complete(status, result.ItemsProcessed, result.ErrorMessage, completedAt);
        connection.ApplySyncResult(status, result.ErrorMessage, tenantContext.UserId, completedAt);

        dbContext.IntegrationSyncRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(run);
    }

    private static IntegrationConnectionResponse ToResponse(IntegrationConnection connection, int failedSyncs)
    {
        return new IntegrationConnectionResponse(
            connection.Id,
            connection.Provider,
            connection.DisplayName,
            connection.Status.ToString(),
            connection.ConnectedAt,
            connection.LastSyncAt,
            connection.LastSuccessfulSyncAt,
            connection.LastFailedSyncAt,
            failedSyncs,
            connection.LastError);
    }

    private static IntegrationSyncRunResponse ToResponse(IntegrationSyncRun run)
    {
        return new IntegrationSyncRunResponse(
            run.Id,
            run.ConnectionId,
            run.Provider,
            run.Status.ToString(),
            run.TriggeredBy,
            run.RetriedFromSyncRunId,
            run.StartedAt,
            run.CompletedAt,
            run.ItemsProcessed,
            run.ErrorMessage);
    }

    private static Task WriteAuditAsync(
        IAuditEventWriter auditEventWriter,
        IntegrationConnection connection,
        ITenantContext tenantContext,
        HttpContext httpContext,
        string action,
        CancellationToken cancellationToken)
    {
        var request = new AuditEventRequest(
            connection.TenantId,
            tenantContext.UserId,
            "User",
            "IntegrationConnection",
            connection.Id.ToString(),
            action,
            null,
            $$"""{"provider":"{{connection.Provider}}","status":"{{connection.Status}}"}""",
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            httpContext.TraceIdentifier);

        return auditEventWriter.WriteAsync(request, cancellationToken);
    }
}
