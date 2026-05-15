using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Integrations;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Integrations;
using NorvixHub.Domain.Integrations;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class IntegrationEndpoints
{
    private static async Task<IResult> ListIntegrations(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        foreach (var provider in KnownProviders.Keys)
        {
            await GetOrCreateConnectionAsync(provider, tenantContext, dbContext, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var failedCounts = await dbContext.IntegrationSyncRuns
            .Where(run => run.TenantId == tenantId && run.Status == IntegrationSyncStatus.Failed)
            .GroupBy(run => run.ConnectionId)
            .Select(group => new { ConnectionId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.ConnectionId, item => item.Count, cancellationToken);

        var connections = await dbContext.IntegrationConnections
            .Where(connection => connection.TenantId == tenantId)
            .OrderBy(connection => connection.DisplayName)
            .ToListAsync(cancellationToken);

        return Results.Ok(connections.Select(connection =>
            ToResponse(connection, failedCounts.GetValueOrDefault(connection.Id))));
    }

    private static async Task<IResult> GetIntegration(
        string provider,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!IsKnownProvider(provider))
        {
            return Results.NotFound();
        }

        var connection = await GetOrCreateConnectionAsync(provider, tenantContext, dbContext, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var failedSyncs = await CountFailedSyncsAsync(connection, dbContext, cancellationToken);
        return Results.Ok(ToResponse(connection, failedSyncs));
    }

    private static async Task<IResult> ConnectIntegration(
        string provider,
        ConnectIntegrationRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageIntegrations(tenantContext) || !IsKnownProvider(provider))
        {
            return !IsKnownProvider(provider) ? Results.NotFound() : Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var settingsJson = string.IsNullOrWhiteSpace(request.SettingsJson) ? "{}" : request.SettingsJson.Trim();
        if (!IsSmallJsonObject(settingsJson))
        {
            return Results.BadRequest(new { error = "SettingsJson must be a small JSON object." });
        }

        var connection = await GetOrCreateConnectionAsync(provider, tenantContext, dbContext, cancellationToken);
        connection.Connect(settingsJson, tenantContext.UserId, DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, connection, tenantContext, httpContext, "IntegrationConnected", cancellationToken);
        return Results.Ok(ToResponse(connection, await CountFailedSyncsAsync(connection, dbContext, cancellationToken)));
    }

    private static async Task<IResult> DisconnectIntegration(
        string provider,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageIntegrations(tenantContext) || !IsKnownProvider(provider))
        {
            return !IsKnownProvider(provider) ? Results.NotFound() : Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var connection = await GetOrCreateConnectionAsync(provider, tenantContext, dbContext, cancellationToken);
        connection.Disconnect(tenantContext.UserId, DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, connection, tenantContext, httpContext, "IntegrationDisconnected", cancellationToken);
        return Results.Ok(ToResponse(connection, await CountFailedSyncsAsync(connection, dbContext, cancellationToken)));
    }

    private static bool IsSmallJsonObject(string settingsJson)
    {
        if (settingsJson.Length > 2_000)
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(settingsJson);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
