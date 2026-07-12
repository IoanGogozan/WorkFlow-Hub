using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.SharePoint;

public sealed class SimulatedSharePointDocumentItem : TenantScopedEntity
{
    public required string SiteId { get; init; }
    public required string DriveId { get; init; }
    public Guid DocumentId { get; init; }
    public Guid DocumentVersionId { get; init; }
    public Guid? CaseId { get; init; }
    public required string ExternalItemId { get; init; }
    public required string ParentPath { get; init; }
    public required string Name { get; init; }
    public required string ETag { get; init; }
    public required string Version { get; init; }
    public required string MetadataJson { get; init; }
    public required string SyncStatus { get; init; }
    public required string IdempotencyKey { get; init; }
    public DateTimeOffset LastSyncedAt { get; init; } = DateTimeOffset.UtcNow;
}
