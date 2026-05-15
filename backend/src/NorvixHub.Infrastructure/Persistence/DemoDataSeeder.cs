using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;

namespace NorvixHub.Infrastructure.Persistence;

public sealed class DemoDataSeeder(NorvixHubDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Tenants.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Tenants.Add(CreateTenant());
        dbContext.Users.Add(CreateUser());
        dbContext.TenantMemberships.Add(CreateMembership());
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

