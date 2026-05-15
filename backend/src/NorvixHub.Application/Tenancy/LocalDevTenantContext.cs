using NorvixHub.Domain.Users;

namespace NorvixHub.Application.Tenancy;

public sealed class LocalDevTenantContext : ITenantContext
{
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid DemoUserId = Guid.Parse("22222222-2222-4222-8222-222222222222");

    public Guid TenantId => DemoTenantId;
    public Guid UserId => DemoUserId;
    public TenantRole Role => TenantRole.TenantOwner;
}

