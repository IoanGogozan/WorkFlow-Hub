namespace NorvixHub.Contracts.Cases;

public sealed record CaseListItemResponse(
    Guid Id,
    string CaseNumber,
    string Title,
    string Status,
    Guid? OwnerUserId,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt);

