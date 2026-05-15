using NorvixHub.Application.Integrations;
using NorvixHub.Domain.Integrations;
using System.Text.Json;

namespace NorvixHub.Infrastructure.Integrations;

public sealed class MockIntegrationSyncAdapter : IIntegrationSyncAdapter
{
    public IReadOnlyCollection<string> SupportedProviders { get; } = new[]
    {
        "brreg",
        "microsoft-graph",
        "tripletex",
        "powerbi-fabric"
    };

    public Task<IntegrationSyncResult> SyncAsync(
        IntegrationConnection connection,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ShouldForceFailure(connection.SettingsJson))
        {
            return Task.FromResult(new IntegrationSyncResult(
                false,
                0,
                $"{connection.DisplayName} mock sync failed by configuration."));
        }

        var count = connection.Provider switch
        {
            "tripletex" => 12,
            "microsoft-graph" => 7,
            "powerbi-fabric" => 4,
            _ => 3
        };
        return Task.FromResult(new IntegrationSyncResult(true, count, null));
    }

    private static bool ShouldForceFailure(string settingsJson)
    {
        using var document = JsonDocument.Parse(settingsJson);
        return document.RootElement.TryGetProperty("forceFailure", out var value) &&
            value.ValueKind == JsonValueKind.True;
    }
}
