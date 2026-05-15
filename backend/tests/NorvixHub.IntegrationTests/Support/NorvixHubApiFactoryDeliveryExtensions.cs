using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Delivery;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.IntegrationTests.Support;

public static class NorvixHubApiFactoryDeliveryExtensions
{
    public static async Task<int> CountDeliveryAccessLogsAsync(
        this NorvixHubApiFactory factory,
        Guid packageId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();

        return await dbContext.DeliveryAccessLogs
            .Where(log => log.DeliveryPackageId == packageId)
            .CountAsync();
    }

    public static async Task<string> CreateExpiredDeliveryTokenAsync(
        this NorvixHubApiFactory factory,
        Guid packageId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();

        var token = $"expired-{Guid.NewGuid():N}";
        dbContext.DeliveryLinks.Add(new DeliveryLink
        {
            TenantId = LocalDevTenantContext.DemoTenantId,
            DeliveryPackageId = packageId,
            TokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token))),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        });
        await dbContext.SaveChangesAsync();
        return token;
    }
}
