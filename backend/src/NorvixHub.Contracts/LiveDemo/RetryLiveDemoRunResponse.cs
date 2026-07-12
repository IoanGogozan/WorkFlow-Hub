namespace NorvixHub.Contracts.LiveDemo;

public sealed record RetryLiveDemoRunResponse(
    Guid RunId,
    string Status,
    string PollUrl,
    int RetryCount,
    DateTimeOffset QueuedAt);
