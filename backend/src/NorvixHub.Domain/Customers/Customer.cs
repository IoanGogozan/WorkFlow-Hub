using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Customers;

public sealed class Customer : TenantScopedEntity
{
    public required string Name { get; set; }
    public required string OrganizationNumber { get; init; }
    public string? BrregDataJson { get; set; }
    public string Source { get; set; } = "Brreg";
    public DateTimeOffset SourceUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactEmail { get; set; }
}
