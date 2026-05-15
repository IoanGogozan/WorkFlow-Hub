using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Integrations;

public sealed class IntegrationSyncRun : TenantScopedEntity
{
    public Guid ConnectionId { get; init; }
    public required string Provider { get; init; }
    public IntegrationSyncStatus Status { get; private set; } = IntegrationSyncStatus.Running;
    public string TriggeredBy { get; init; } = "Manual";
    public Guid? RetriedFromSyncRunId { get; init; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; private set; }
    public int ItemsProcessed { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void Complete(IntegrationSyncStatus status, int itemsProcessed, string? errorMessage, DateTimeOffset now)
    {
        Status = status;
        ItemsProcessed = itemsProcessed;
        ErrorMessage = errorMessage;
        CompletedAt = now;
    }
}
