using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Delivery;

public sealed class DeliveryAccessLog : TenantScopedEntity
{
    public Guid DeliveryLinkId { get; init; }
    public Guid DeliveryPackageId { get; init; }
    public Guid? DocumentId { get; init; }
    public string Action { get; init; } = "Viewed";
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTimeOffset AccessedAt { get; init; } = DateTimeOffset.UtcNow;
}
