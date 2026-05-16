using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.AI;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.Integrations;
using NorvixHub.Application.Organizations;
using NorvixHub.Infrastructure.AI;
using NorvixHub.Infrastructure.Audit;
using NorvixHub.Infrastructure.Documents;
using NorvixHub.Infrastructure.Integrations;
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
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IDocumentClassificationProvider, MockDocumentClassificationProvider>();
        services.AddScoped<IIntegrationSyncAdapter, MockIntegrationSyncAdapter>();
        services.Configure<LocalFileStorageOptions>(options =>
            configuration.GetSection("Storage:Local").Bind(options));
        services.Configure<BrregOptions>(options =>
            configuration.GetSection("Brreg").Bind(options));
        services.AddHttpClient<IBrregClient, BrregClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BrregOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        return services;
    }
}
