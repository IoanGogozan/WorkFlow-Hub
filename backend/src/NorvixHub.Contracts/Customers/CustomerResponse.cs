namespace NorvixHub.Contracts.Customers;

public sealed record CustomerResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string OrganizationNumber,
    string Source,
    DateTimeOffset SourceUpdatedAt,
    DateTimeOffset CreatedAt);

