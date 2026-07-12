namespace NorvixHub.Contracts.LiveDemo;

public sealed record LiveDemoRunResponse(
    Guid RunId,
    string Status,
    string? CurrentStepKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    long? TotalDurationMs,
    int RetryCount,
    bool CanRetry,
    string? PublicErrorCode,
    string? PublicErrorMessage,
    IReadOnlyList<LiveDemoRunStepResponse> Steps,
    LiveDemoRunResultResponse? Result);
