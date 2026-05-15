namespace NorvixHub.Application.Documents;

public sealed record DocumentClassificationSuggestion(
    string DocumentType,
    DateOnly? ExpiryDate,
    string Summary,
    decimal Confidence);

