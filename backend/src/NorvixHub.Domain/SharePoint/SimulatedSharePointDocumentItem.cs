using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.SharePoint;

public sealed class SimulatedSharePointDocumentItem : TenantScopedEntity
{
    public required string SiteId { get; init; }
    public required string DriveId { get; init; }
    public Guid DocumentId { get; init; }
    public Guid DocumentVersionId { get; set; }
    public Guid? CaseId { get; init; }
    public required string ExternalItemId { get; init; }
    public required string ParentPath { get; init; }
    public required string Name { get; init; }
    public required string ETag { get; set; }
    public required string Version { get; set; }
    public required string MetadataJson { get; set; }
    public required string SyncStatus { get; set; }
    public required string IdempotencyKey { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
