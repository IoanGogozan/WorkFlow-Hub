using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace NorvixHub.IntegrationTests.Support;

public sealed class NorvixHubApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly Guid SecondTenantId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    public static readonly Guid DisabledUserId = Guid.Parse("44444444-4444-4444-8444-444444444444");

    private readonly PostgreSqlContainer? _postgres;
    private readonly string? _externalConnectionString;

    public NorvixHubApiFactory()
    {
        _externalConnectionString = Environment.GetEnvironmentVariable("NORVIXHUB_TEST_POSTGRES");

        if (string.IsNullOrWhiteSpace(_externalConnectionString))
        {
            _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("norvixhub_tests")
        .WithUsername("norvixhub")
        .WithPassword("norvixhub_dev_password")
        .Build();
        }
    }

    public async ValueTask InitializeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.StartAsync();
        }
    }

    public new async ValueTask DisposeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    public async Task SeedExtraTenantsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();

        if (!dbContext.Tenants.Any(tenant => tenant.Id == SecondTenantId))
        {
            dbContext.Tenants.Add(new Tenant
            {
                Id = SecondTenantId,
                Name = "Fjord Kontroll AS",
                Slug = "fjord-kontroll",
                OrganizationNumber = "888888888"
            });
        }

        if (!dbContext.Users.Any(user => user.Id == DisabledUserId))
        {
            dbContext.Users.Add(new UserProfile
            {
                Id = DisabledUserId,
                DisplayName = "Disabled User",
                Email = "disabled.user@example.test",
                IsActive = false
            });
        }

        if (!dbContext.TenantMemberships.Any(membership =>
            membership.TenantId == LocalDevTenantContext.DemoTenantId &&
            membership.UserId == DisabledUserId))
        {
            dbContext.TenantMemberships.Add(new TenantMembership
            {
                TenantId = LocalDevTenantContext.DemoTenantId,
                UserId = DisabledUserId,
                Role = TenantRole.Admin
            });
        }

        await dbContext.SaveChangesAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = GetConnectionString(),
                ["Database:ApplyMigrationsOnStartup"] = "true"
            });
        });
    }

    private string GetConnectionString()
    {
        return _externalConnectionString ?? _postgres!.GetConnectionString();
    }
}
