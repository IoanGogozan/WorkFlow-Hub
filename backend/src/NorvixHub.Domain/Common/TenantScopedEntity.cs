namespace NorvixHub.Domain.Common;

public abstract class TenantScopedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; private set; }

    public void MarkUpdated(Guid? userId, DateTimeOffset now)
    {
        UpdatedBy = userId;
        UpdatedAt = now;
    }
}

