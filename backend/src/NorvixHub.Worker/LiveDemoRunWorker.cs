using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NorvixHub.Application.Audit;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Infrastructure.LiveDemo;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Worker;

public sealed class LiveDemoRunWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<LiveDemoOptions> options,
    ILogger<LiveDemoRunWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Live demo processing is disabled.");
            return;
        }

        var interval = TimeSpan.FromMilliseconds(Math.Max(100, options.Value.WorkerPollMilliseconds));
        using var timer = new PeriodicTimer(interval);
        do
        {
            await RunOnceAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        await RecoverStaleRunsAsync(cancellationToken);

        Guid? runId;
        using (var scope = scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
            runId = await dbContext.LiveDemoRuns
                .Where(run => run.Status == LiveDemoRunStatus.Queued)
                .OrderBy(run => run.CreatedAt)
                .Select(run => (Guid?)run.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (runId is null)
        {
            return;
        }

        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<ILiveDemoRunProcessor>();
            await processor.ProcessAsync(runId.Value, cancellationToken);
            var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
            var run = await dbContext.LiveDemoRuns.AsNoTracking().SingleAsync(
                candidate => candidate.Id == runId.Value,
                cancellationToken);
            logger.LogInformation(
                "Processed live demo run {RunId} with status {Status}, step {Step}, in {DurationMs} ms.",
                run.Id,
                run.Status,
                run.CurrentStepKey,
                (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Live demo run {RunId} failed in worker.", runId.Value);
            await MarkRunFailedAsync(runId.Value, cancellationToken);
        }
    }

    private async Task RecoverStaleRunsAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-Math.Max(1, options.Value.RunRecoveryMinutes));
        Guid[] staleRunIds;
        using (var scope = scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
            staleRunIds = await dbContext.LiveDemoRuns
                .Where(run => run.Status == LiveDemoRunStatus.Running && run.UpdatedAt <= cutoff)
                .OrderBy(run => run.UpdatedAt)
                .Select(run => run.Id)
                .ToArrayAsync(cancellationToken);
        }

        foreach (var runId in staleRunIds)
        {
            await MarkRunFailedAsync(runId, cancellationToken, queueRetry: true);
        }
    }

    private async Task MarkRunFailedAsync(Guid runId, CancellationToken cancellationToken, bool queueRetry = false)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var auditEventWriter = scope.ServiceProvider.GetRequiredService<IAuditEventWriter>();
        var run = await dbContext.LiveDemoRuns.SingleOrDefaultAsync(
            candidate => candidate.Id == runId,
            cancellationToken);
        if (run is null || run.Status is LiveDemoRunStatus.Completed or LiveDemoRunStatus.Failed)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (run.Status == LiveDemoRunStatus.Queued)
        {
            run.MarkRunning(run.CurrentStepKey ?? "request-created", now);
        }

        run.MarkFailed("RUN_PROCESSING_FAILED", "Live-demoen kunne ikke fullføres.", now);
        if (queueRetry && run.RetryCount < Math.Max(0, options.Value.MaxRetriesPerRun))
        {
            run.QueueRetry(now, Math.Max(0, options.Value.MaxRetriesPerRun));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditEventWriter.WriteAsync(
            new AuditEventRequest(
                run.TenantId,
                run.CreatedBy,
                "LiveDemoWorker",
                "LiveDemoRun",
                run.Id.ToString(),
                queueRetry && run.Status == LiveDemoRunStatus.Queued
                    ? "LiveDemoRunRecovered"
                    : "LiveDemoRunFailed",
                null,
                null,
                null,
                null,
                run.CorrelationId),
            cancellationToken);
    }
}
