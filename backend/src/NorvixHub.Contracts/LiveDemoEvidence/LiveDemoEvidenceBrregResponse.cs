namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceBrregResponse(
    string Mode,
    string OrganizationNumber,
    string OrganizationName,
    long? LookupDurationMs,
    DateTimeOffset? SourceUpdatedAt,
    string StatusMessage);
