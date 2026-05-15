using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Cases;

public sealed class CaseWorkspace : TenantScopedEntity
{
    public required string CaseNumber { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public Guid? CustomerId { get; init; }
    public CaseStatus Status { get; private set; } = CaseStatus.Open;
    public Guid? OwnerUserId { get; init; }
    public DateOnly? DueDate { get; init; }
    public string? MissingInformationJson { get; init; }
    public string? ExternalProjectId { get; init; }
    public Guid? SourceIntakeItemId { get; init; }
}

