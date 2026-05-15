using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Integrations;

public sealed class IntegrationConnection : TenantScopedEntity
{
    public required string Provider { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public IntegrationConnectionStatus Status { get; private set; } = IntegrationConnectionStatus.Disconnected;
    public DateTimeOffset? ConnectedAt { get; private set; }
    public DateTimeOffset? LastSyncAt { get; private set; }
    public DateTimeOffset? LastSuccessfulSyncAt { get; private set; }
    public DateTimeOffset? LastFailedSyncAt { get; private set; }
    public string? LastError { get; private set; }
    public string SettingsJson { get; private set; } = "{}";

    public void Connect(string settingsJson, Guid? userId, DateTimeOffset now)
    {
        SettingsJson = settingsJson;
        Status = IntegrationConnectionStatus.Connected;
        ConnectedAt = now;
        LastError = null;
        MarkUpdated(userId, now);
    }

    public void Disconnect(Guid? userId, DateTimeOffset now)
    {
        Status = IntegrationConnectionStatus.Disconnected;
        MarkUpdated(userId, now);
    }

    public void ApplySyncResult(IntegrationSyncStatus status, string? error, Guid? userId, DateTimeOffset now)
    {
        LastSyncAt = now;
        LastError = error;
        if (status == IntegrationSyncStatus.Succeeded)
        {
            LastSuccessfulSyncAt = now;
            Status = IntegrationConnectionStatus.Connected;
        }
        else
        {
            LastFailedSyncAt = now;
            Status = IntegrationConnectionStatus.Error;
        }

        MarkUpdated(userId, now);
    }
}
