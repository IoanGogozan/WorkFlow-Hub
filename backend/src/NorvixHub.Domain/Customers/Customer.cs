using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Customers;

public sealed class Customer : TenantScopedEntity
{
    public required string Name { get; init; }
    public required string OrganizationNumber { get; init; }
    public string? BrregDataJson { get; init; }
    public string Source { get; init; } = "Brreg";
    public DateTimeOffset SourceUpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string? PrimaryContactName { get; init; }
    public string? PrimaryContactEmail { get; init; }
}

