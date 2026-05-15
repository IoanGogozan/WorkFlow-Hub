namespace NorvixHub.Contracts.Integrations;

public sealed record IntegrationSyncRunResponse(
    Guid Id,
    Guid ConnectionId,
    string Provider,
    string Status,
    string TriggeredBy,
    Guid? RetriedFromSyncRunId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int ItemsProcessed,
    string? ErrorMessage);
