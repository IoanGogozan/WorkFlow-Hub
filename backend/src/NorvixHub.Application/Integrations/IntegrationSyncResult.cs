namespace NorvixHub.Application.Integrations;

public sealed record IntegrationSyncResult(
    bool Succeeded,
    int ItemsProcessed,
    string? ErrorMessage);
