namespace NorvixHub.Contracts.Integrations;

public sealed record IntegrationConnectionResponse(
    Guid Id,
    string Provider,
    string DisplayName,
    string Status,
    DateTimeOffset? ConnectedAt,
    DateTimeOffset? LastSyncAt,
    DateTimeOffset? LastSuccessfulSyncAt,
    DateTimeOffset? LastFailedSyncAt,
    int FailedSyncs,
    string? LastError);
