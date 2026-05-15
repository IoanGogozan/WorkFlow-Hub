using NorvixHub.Domain.Users;

namespace NorvixHub.Application.Tenancy;

public sealed class TenantAuthorizationService(ITenantContext tenantContext)
{
    public bool HasAnyRole(params TenantRole[] allowedRoles)
    {
        return tenantContext.Role is { } role && allowedRoles.Contains(role);
    }

    public bool CanManageIntegrations()
    {
        return HasAnyRole(TenantRole.TenantOwner, TenantRole.Admin);
    }
}

