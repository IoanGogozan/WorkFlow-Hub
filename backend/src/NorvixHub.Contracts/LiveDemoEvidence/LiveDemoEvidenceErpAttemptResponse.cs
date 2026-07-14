namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceErpAttemptResponse(
    DateTimeOffset Timestamp,
    int Attempt,
    string Status,
    long? DurationMs,
    string Message);
