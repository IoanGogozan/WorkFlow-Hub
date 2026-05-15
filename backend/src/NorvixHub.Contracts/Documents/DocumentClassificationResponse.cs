namespace NorvixHub.Contracts.Documents;

public sealed record DocumentClassificationResponse(
    Guid AiAnalysisRunId,
    string DocumentType,
    DateOnly? ExpiryDate,
    string Summary,
    decimal Confidence);

