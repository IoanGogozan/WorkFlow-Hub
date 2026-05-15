namespace NorvixHub.Contracts.Documents;

public sealed record ApproveDocumentClassificationRequest(
    Guid AiAnalysisRunId,
    string DocumentType,
    DateOnly? ExpiryDate);

