using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Documents;

public sealed class DocumentLink : TenantScopedEntity
{
    public Guid DocumentId { get; init; }
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
}

