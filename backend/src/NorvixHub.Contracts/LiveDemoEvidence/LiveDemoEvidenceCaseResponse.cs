namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceCaseResponse(
    string CaseNumber,
    string Title,
    string Status,
    string CustomerName,
    DateTimeOffset CreatedAt,
    string CaseHref);
