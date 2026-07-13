namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceErpResponse(
    string Mode,
    string Status,
    string? ExternalReceiptId,
    string? IdempotencyKey,
    int Attempts,
    long? LastDurationMs,
    string? SafeError);
