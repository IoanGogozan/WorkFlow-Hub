namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceSharePointOperationResponse(
    DateTimeOffset Timestamp,
    string Method,
    string Action,
    int StatusCode,
    string Result,
    long DurationMs,
    int Attempt,
    string IdempotencyResult);
