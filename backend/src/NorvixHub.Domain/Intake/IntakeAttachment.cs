using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Intake;

public sealed class IntakeAttachment : TenantScopedEntity
{
    public Guid IntakeItemId { get; init; }
    public Guid? DocumentId { get; init; }
    public required string OriginalFilename { get; init; }
    public required string ContentType { get; init; }
    public long SizeBytes { get; init; }
}

