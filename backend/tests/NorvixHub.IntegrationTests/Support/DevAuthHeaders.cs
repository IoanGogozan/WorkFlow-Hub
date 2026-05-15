using NorvixHub.Application.Tenancy;

namespace NorvixHub.IntegrationTests.Support;

public static class DevAuthHeaders
{
    public static void AddDemoAdmin(HttpRequestMessage request)
    {
        Add(request, LocalDevTenantContext.DemoTenantId, LocalDevTenantContext.DemoUserId);
    }

    public static void Add(HttpRequestMessage request, Guid tenantId, Guid userId)
    {
        request.Headers.Add("X-Norvix-Tenant-Id", tenantId.ToString());
        request.Headers.Add("X-Norvix-User-Id", userId.ToString());
    }
}

