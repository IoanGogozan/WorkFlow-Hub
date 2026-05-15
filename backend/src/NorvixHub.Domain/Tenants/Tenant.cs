namespace NorvixHub.Domain.Tenants;

public sealed class Tenant
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? OrganizationNumber { get; init; }
    public string CountryCode { get; init; } = "NO";
    public bool AiEnabled { get; init; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

