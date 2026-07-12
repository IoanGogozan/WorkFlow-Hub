using NorvixHub.Application.Integrations;
using NorvixHub.Domain.Integrations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Infrastructure.Integrations;

public sealed class MockIntegrationSyncAdapter(NorvixHubDbContext dbContext) : IIntegrationSyncAdapter
{
    public IReadOnlyCollection<string> SupportedProviders { get; } = new[]
    {
        "brreg",
        "microsoft-graph",
        "tripletex",
        "powerbi-fabric"
    };

    public async Task<IntegrationSyncResult> SyncAsync(
        IntegrationConnection connection,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ShouldForceFailure(connection.SettingsJson))
        {
            return new IntegrationSyncResult(
                false,
                0,
                $"{connection.DisplayName} mock sync failed by configuration.");
        }

        var count = connection.Provider switch
        {
            "tripletex" => 12,
            "microsoft-graph" => await dbContext.SimulatedSharePointOperations.CountAsync(
                operation => operation.TenantId == connection.TenantId && operation.Succeeded,
                cancellationToken),
            "powerbi-fabric" => 4,
            _ => 3
        };
        return new IntegrationSyncResult(true, count, null);
    }

    private static bool ShouldForceFailure(string settingsJson)
    {
        using var document = JsonDocument.Parse(settingsJson);
        return document.RootElement.TryGetProperty("forceFailure", out var value) &&
            value.ValueKind == JsonValueKind.True;
    }
}
