using NorvixHub.Domain.Users;

namespace NorvixHub.Application.Tenancy;

public sealed class LocalDevTenantContext : ITenantContext
{
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid DemoUserId = Guid.Parse("22222222-2222-4222-8222-222222222222");

    public bool IsAuthenticated { get; private set; }
    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public TenantRole? Role { get; private set; }

    public void SetAuthenticated(Guid tenantId, Guid userId, TenantRole role)
    {
        IsAuthenticated = true;
        TenantId = tenantId;
        UserId = userId;
        Role = role;
    }
}
