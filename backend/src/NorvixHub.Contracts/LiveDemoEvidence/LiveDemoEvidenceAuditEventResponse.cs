namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceAuditEventResponse(
    DateTimeOffset Timestamp,
    string EventType,
    string OperationLabel,
    string EntityType,
    string Result,
    string CorrelationId);
