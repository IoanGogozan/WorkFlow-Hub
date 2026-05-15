using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Delivery;

public sealed class DeliveryLink : TenantScopedEntity
{
    public Guid DeliveryPackageId { get; init; }
    public required string TokenHash { get; init; }
    public string? RecipientEmail { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? RevokedBy { get; private set; }

    public bool IsActive(DateTimeOffset now)
    {
        return RevokedAt is null && ExpiresAt > now;
    }

    public void Revoke(Guid? userId, DateTimeOffset now)
    {
        RevokedBy = userId;
        RevokedAt = now;
        MarkUpdated(userId, now);
    }
}
