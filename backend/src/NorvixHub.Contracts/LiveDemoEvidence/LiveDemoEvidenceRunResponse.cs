namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceRunResponse(
    Guid RunId,
    string Status,
    string CorrelationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    long? TotalDurationMs,
    int RetryCount,
    string ScenarioLabel);
