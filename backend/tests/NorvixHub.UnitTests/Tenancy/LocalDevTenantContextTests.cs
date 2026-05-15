using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Users;
using Xunit;

namespace NorvixHub.UnitTests.Tenancy;

public sealed class LocalDevTenantContextTests
{
    [Fact]
    public void Local_dev_context_uses_demo_tenant_owner()
    {
        var context = new LocalDevTenantContext();

        context.TenantId.Should().Be(LocalDevTenantContext.DemoTenantId);
        context.UserId.Should().Be(LocalDevTenantContext.DemoUserId);
        context.Role.Should().Be(TenantRole.TenantOwner);
    }
}
