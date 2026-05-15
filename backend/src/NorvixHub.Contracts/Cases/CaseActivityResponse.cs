namespace NorvixHub.Contracts.Cases;

public sealed record CaseActivityResponse(
    Guid Id,
    string EntityType,
    string EntityId,
    string Action,
    Guid? ActorUserId,
    DateTimeOffset CreatedAt);

