using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.AI;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Organizations;
using NorvixHub.Infrastructure.AI;
using NorvixHub.Infrastructure.Audit;
using NorvixHub.Infrastructure.Organizations;
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
        services.AddScoped<IAiReviewProvider, MockAiReviewProvider>();
        services.Configure<BrregOptions>(options =>
            configuration.GetSection("Brreg").Bind(options));
        services.AddHttpClient<IBrregClient, BrregClient>();
        return services;
    }
}
