namespace NorvixHub.Contracts.Cases;

public sealed record CaseResponse(
    Guid Id,
    Guid TenantId,
    string CaseNumber,
    string Title,
    string? Description,
    string Status,
    Guid? OwnerUserId,
    DateOnly? DueDate,
    Guid? SourceIntakeItemId,
    DateTimeOffset CreatedAt);

