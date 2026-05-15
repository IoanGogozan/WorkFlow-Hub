using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Delivery;

public sealed class DeliveryPackageItem : TenantScopedEntity
{
    public Guid DeliveryPackageId { get; init; }
    public Guid DocumentId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}
