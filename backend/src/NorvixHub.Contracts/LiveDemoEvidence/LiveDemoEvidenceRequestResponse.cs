namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceRequestResponse(
    string Title,
    string Body,
    string CustomerReference,
    string SourceLabel,
    DateTimeOffset CreatedAt);
