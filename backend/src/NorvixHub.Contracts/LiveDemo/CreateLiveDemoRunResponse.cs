namespace NorvixHub.Contracts.LiveDemo;

public sealed record CreateLiveDemoRunResponse(
    Guid RunId,
    string Status,
    string PollUrl,
    DateTimeOffset CreatedAt);
