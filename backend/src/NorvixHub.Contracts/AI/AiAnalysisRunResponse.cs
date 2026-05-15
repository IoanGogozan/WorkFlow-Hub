namespace NorvixHub.Contracts.AI;

public sealed record AiAnalysisRunResponse(
    Guid Id,
    Guid EntityId,
    string EntityType,
    string Provider,
    string Model,
    string PromptVersion,
    decimal Confidence,
    string Status,
    AiIntakeSuggestionResponse Suggestion,
    DateTimeOffset CreatedAt);

