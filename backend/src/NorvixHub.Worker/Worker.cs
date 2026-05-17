using Microsoft.Extensions.Options;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Worker;

public sealed class Worker(
    IServiceScopeFactory scopeFactory,
    IOptions<DemoSessionCleanupOptions> options,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("NorvixHub worker started.");
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Demo session cleanup is disabled.");
            return;
        }

        await RunCleanupAsync(stoppingToken);
        var interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.IntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCleanupAsync(stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var cleanupService = scope.ServiceProvider.GetRequiredService<DemoSessionCleanupService>();
            var result = await cleanupService.CleanupExpiredAsync(cancellationToken);
            if (result.SessionsDeleted > 0)
            {
                logger.LogInformation(
                    "Deleted {SessionsDeleted} expired demo sessions, {TenantsDeleted} tenants, and {UsersDeleted} users.",
                    result.SessionsDeleted,
                    result.TenantsDeleted,
                    result.UsersDeleted);
                logger.LogInformation(
                    "Demo session file cleanup deleted {FilesDeleted} files with {FileDeleteFailures} failures.",
                    result.FilesDeleted,
                    result.FileDeleteFailures);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Demo session cleanup failed.");
        }
    }
}
