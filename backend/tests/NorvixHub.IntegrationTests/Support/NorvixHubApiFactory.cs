using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Cases;
using NorvixHub.Domain.Intake;
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
    public static readonly Guid ViewerUserId = Guid.Parse("55555555-5555-4555-8555-555555555555");

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

        if (!dbContext.Users.Any(user => user.Id == ViewerUserId))
        {
            dbContext.Users.Add(new UserProfile
            {
                Id = ViewerUserId,
                DisplayName = "Viewer User",
                Email = "viewer.user@example.test"
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

        if (!dbContext.TenantMemberships.Any(membership =>
            membership.TenantId == LocalDevTenantContext.DemoTenantId &&
            membership.UserId == ViewerUserId))
        {
            dbContext.TenantMemberships.Add(new TenantMembership
            {
                TenantId = LocalDevTenantContext.DemoTenantId,
                UserId = ViewerUserId,
                Role = TenantRole.Viewer
            });
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<(Guid Id, string Subject)> CreateSecondTenantIntakeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();

        var subject = $"Second tenant intake {Guid.NewGuid():N}";
        var intake = new IntakeItem
        {
            TenantId = SecondTenantId,
            Source = IntakeSource.Manual,
            Subject = subject,
            Body = "This intake belongs to another tenant."
        };

        dbContext.IntakeItems.Add(intake);
        await dbContext.SaveChangesAsync();
        return (intake.Id, subject);
    }

    public async Task<Guid> CreateSecondTenantCaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();

        var caseWorkspace = new CaseWorkspace
        {
            TenantId = SecondTenantId,
            CaseNumber = $"CASE-OTHER-{Guid.NewGuid():N}"[..30],
            Title = "Second tenant case",
            Description = "This case belongs to another tenant."
        };

        dbContext.Cases.Add(caseWorkspace);
        await dbContext.SaveChangesAsync();
        return caseWorkspace.Id;
    }

    public async Task<int> CountAuditEventsAsync(Guid tenantId, string entityType, string action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();

        return await dbContext.AuditEvents
            .Where(auditEvent =>
                auditEvent.TenantId == tenantId &&
                auditEvent.EntityType == entityType &&
                auditEvent.Action == action)
            .CountAsync();
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
