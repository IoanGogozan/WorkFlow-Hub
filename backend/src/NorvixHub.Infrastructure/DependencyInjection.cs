using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.Audit;
using NorvixHub.Infrastructure.Audit;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NorvixHubDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<DemoDataSeeder>();
        services.AddScoped<IAuditEventWriter, DatabaseAuditEventWriter>();
        return services;
    }
}

