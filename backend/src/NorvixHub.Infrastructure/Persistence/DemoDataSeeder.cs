using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;

namespace NorvixHub.Infrastructure.Persistence;

public sealed class DemoDataSeeder(NorvixHubDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!await dbContext.Tenants.AnyAsync(
            tenant => tenant.Id == LocalDevTenantContext.DemoTenantId,
            cancellationToken))
        {
            dbContext.Tenants.Add(CreateTenant());
        }

        if (!await dbContext.Users.AnyAsync(
            user => user.Id == LocalDevTenantContext.DemoUserId,
            cancellationToken))
        {
            dbContext.Users.Add(CreateUser());
        }

        if (!await dbContext.TenantMemberships.AnyAsync(
            membership =>
                membership.TenantId == LocalDevTenantContext.DemoTenantId &&
                membership.UserId == LocalDevTenantContext.DemoUserId,
            cancellationToken))
        {
            dbContext.TenantMemberships.Add(CreateMembership());
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Tenant CreateTenant() => new()
    {
        Id = LocalDevTenantContext.DemoTenantId,
        Name = "Agder Drift & Service AS",
        Slug = "agder-drift-service",
        OrganizationNumber = "999999999"
    };

    private static UserProfile CreateUser() => new()
    {
        Id = LocalDevTenantContext.DemoUserId,
        DisplayName = "Demo Admin",
        Email = "demo.admin@agder-drift.example"
    };

    private static TenantMembership CreateMembership() => new()
    {
        TenantId = LocalDevTenantContext.DemoTenantId,
        UserId = LocalDevTenantContext.DemoUserId,
        Role = TenantRole.TenantOwner
    };
}
