using NorvixHub.Domain.Integrations;

namespace NorvixHub.Application.Integrations;

public interface IIntegrationSyncAdapter
{
    IReadOnlyCollection<string> SupportedProviders { get; }
    Task<IntegrationSyncResult> SyncAsync(
        IntegrationConnection connection,
        CancellationToken cancellationToken);
}
