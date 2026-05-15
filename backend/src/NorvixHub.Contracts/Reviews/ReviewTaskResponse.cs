namespace NorvixHub.Contracts.Reviews;

public sealed record ReviewTaskResponse(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string ReviewType,
    string Status,
    Guid? AiAnalysisRunId,
    DateTimeOffset CreatedAt);

