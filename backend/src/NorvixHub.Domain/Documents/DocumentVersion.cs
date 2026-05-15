using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Documents;

public sealed class DocumentVersion : TenantScopedEntity
{
    public Guid DocumentId { get; init; }
    public int VersionNumber { get; init; }
    public required string BlobContainer { get; init; }
    public required string BlobName { get; init; }
    public required string OriginalFilename { get; init; }
    public required string ContentType { get; init; }
    public long SizeBytes { get; init; }
    public required string Sha256Hash { get; init; }
    public Guid? UploadedByUserId { get; init; }
}

