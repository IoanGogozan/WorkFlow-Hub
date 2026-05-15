namespace NorvixHub.Contracts.AI;

public sealed record ApproveAiSuggestionRequest(
    Guid AiAnalysisRunId,
    string? CustomerName,
    string? OrganizationNumber,
    string? Category,
    string? Urgency);

