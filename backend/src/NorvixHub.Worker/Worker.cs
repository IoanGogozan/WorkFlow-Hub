namespace NorvixHub.Worker;

public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("NorvixHub worker started.");
        return Task.CompletedTask;
    }
}

