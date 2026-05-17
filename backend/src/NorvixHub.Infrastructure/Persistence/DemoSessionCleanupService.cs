using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorvixHub.Application.Documents;

namespace NorvixHub.Infrastructure.Persistence;

public sealed class DemoSessionCleanupService(
    NorvixHubDbContext dbContext,
    IFileStorage fileStorage,
    IOptions<DemoSessionCleanupOptions> options,
    ILogger<DemoSessionCleanupService> logger)
{
    public async Task<DemoSessionCleanupResult> CleanupExpiredAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddMinutes(-Math.Max(0, options.Value.RetentionGraceMinutes));
        var batchSize = Math.Clamp(options.Value.BatchSize, 1, 500);

        var sessions = await dbContext.DemoSessions
            .Where(session => session.ExpiresAt <= cutoff)
            .OrderBy(session => session.ExpiresAt)
            .Select(session => new { session.Id, session.TenantId, session.UserId })
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return new DemoSessionCleanupResult(0, 0, 0, 0, 0);
        }

        var sessionIds = sessions.Select(session => session.Id).ToArray();
        var tenantIds = sessions.Select(session => session.TenantId).Distinct().ToArray();
        var userIds = sessions.Select(session => session.UserId).Distinct().ToArray();
        var fileCleanup = await DeleteStoredFilesAsync(tenantIds, cancellationToken);

        await DeleteTenantScopedDataAsync(tenantIds, cancellationToken);
        await dbContext.TenantMemberships
            .Where(membership => tenantIds.Contains(membership.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.DemoSessions
            .Where(session => sessionIds.Contains(session.Id))
            .ExecuteDeleteAsync(cancellationToken);
        var tenantsDeleted = await dbContext.Tenants
            .Where(tenant => tenantIds.Contains(tenant.Id))
            .ExecuteDeleteAsync(cancellationToken);
        var usersDeleted = await dbContext.Users
            .Where(user => userIds.Contains(user.Id) &&
                !dbContext.TenantMemberships.Any(membership => membership.UserId == user.Id))
            .ExecuteDeleteAsync(cancellationToken);

        return new DemoSessionCleanupResult(
            sessionIds.Length,
            tenantsDeleted,
            usersDeleted,
            fileCleanup.FilesDeleted,
            fileCleanup.FileDeleteFailures);
    }

    private async Task<FileCleanupResult> DeleteStoredFilesAsync(
        Guid[] tenantIds,
        CancellationToken cancellationToken)
    {
        var storedFiles = await dbContext.DocumentVersions
            .Where(version => tenantIds.Contains(version.TenantId))
            .Select(version => new StoredFileReference(version.BlobContainer, version.BlobName))
            .Distinct()
            .ToListAsync(cancellationToken);

        var deleted = 0;
        var failures = 0;
        foreach (var storedFile in storedFiles)
        {
            try
            {
                await fileStorage.DeleteAsync(storedFile.Container, storedFile.BlobName, cancellationToken);
                deleted++;
                logger.LogInformation(
                    "Deleted demo session file {Container}/{BlobName}.",
                    storedFile.Container,
                    storedFile.BlobName);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                failures++;
                logger.LogWarning(
                    exception,
                    "Failed to delete demo session file {Container}/{BlobName}. Continuing cleanup.",
                    storedFile.Container,
                    storedFile.BlobName);
            }
        }

        return new FileCleanupResult(deleted, failures);
    }

    private async Task DeleteTenantScopedDataAsync(Guid[] tenantIds, CancellationToken cancellationToken)
    {
        await dbContext.DeliveryAccessLogs
            .Where(log => tenantIds.Contains(log.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.DeliveryLinks
            .Where(link => tenantIds.Contains(link.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.DeliveryPackageItems
            .Where(item => tenantIds.Contains(item.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.DeliveryPackages
            .Where(package => tenantIds.Contains(package.TenantId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.DocumentLinks
            .Where(link => tenantIds.Contains(link.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.DocumentVersions
            .Where(version => tenantIds.Contains(version.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Documents
            .Where(document => tenantIds.Contains(document.TenantId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.CaseNotes
            .Where(note => tenantIds.Contains(note.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.CaseTasks
            .Where(task => tenantIds.Contains(task.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Cases
            .Where(caseWorkspace => tenantIds.Contains(caseWorkspace.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Customers
            .Where(customer => tenantIds.Contains(customer.TenantId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.IntegrationSyncRuns
            .Where(run => tenantIds.Contains(run.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.IntegrationConnections
            .Where(connection => tenantIds.Contains(connection.TenantId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.ReviewTasks
            .Where(task => tenantIds.Contains(task.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.AiAnalysisRuns
            .Where(run => tenantIds.Contains(run.TenantId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.IntakeAttachments
            .Where(attachment => tenantIds.Contains(attachment.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.IntakeItems
            .Where(intake => tenantIds.Contains(intake.TenantId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.AuditEvents
            .Where(auditEvent => tenantIds.Contains(auditEvent.TenantId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private sealed record StoredFileReference(string Container, string BlobName);

    private sealed record FileCleanupResult(int FilesDeleted, int FileDeleteFailures);
}
