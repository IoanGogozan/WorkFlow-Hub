namespace NorvixHub.Contracts.Cases;

public sealed record CaseTaskResponse(
    Guid Id,
    Guid CaseId,
    string Title,
    string? Description,
    string Status,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt);

