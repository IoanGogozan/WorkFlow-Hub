namespace NorvixHub.Contracts.Documents;

public sealed record DocumentResponse(
    Guid Id,
    Guid TenantId,
    string Title,
    string Status,
    string? DocumentType,
    Guid? CurrentVersionId,
    Guid? CaseId,
    DateOnly? ExpiryDate,
    DateTimeOffset CreatedAt);

