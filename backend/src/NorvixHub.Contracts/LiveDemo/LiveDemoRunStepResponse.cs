namespace NorvixHub.Contracts.LiveDemo;

public sealed record LiveDemoRunStepResponse(
    string Key,
    int Sequence,
    string PublicStage,
    string Provider,
    string Status,
    string EvidenceMode,
    int AttemptCount,
    long? DurationMs,
    string? PublicSummary,
    string? PublicEvidenceReference,
    string? PublicErrorCode,
    string? PublicErrorMessage);
