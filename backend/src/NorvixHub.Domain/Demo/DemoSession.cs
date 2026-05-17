namespace NorvixHub.Domain.Demo;

public sealed class DemoSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public required string TokenHash { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? LastSeenAt { get; private set; }
    public DemoSessionStatus Status { get; private set; } = DemoSessionStatus.Active;
    public string? IpHash { get; init; }
    public string? UserAgentHash { get; init; }

    public bool IsActive(DateTimeOffset now)
    {
        return Status == DemoSessionStatus.Active && ExpiresAt > now;
    }

    public void MarkSeen(DateTimeOffset now)
    {
        LastSeenAt = now;
    }

    public void MarkExpired()
    {
        Status = DemoSessionStatus.Expired;
    }
}
