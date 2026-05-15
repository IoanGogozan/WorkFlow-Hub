using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Cases;

public sealed class CaseTask : TenantScopedEntity
{
    public Guid CaseId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public CaseTaskStatus Status { get; private set; } = CaseTaskStatus.Open;
    public Guid? AssignedToUserId { get; init; }
    public DateOnly? DueDate { get; init; }
    public DateTimeOffset? CompletedAt { get; private set; }
}

