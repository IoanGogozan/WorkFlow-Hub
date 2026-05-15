using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Intake;

public sealed class IntakeItem : TenantScopedEntity
{
    public required IntakeSource Source { get; init; }
    public IntakeStatus Status { get; private set; } = IntakeStatus.New;
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? CustomerName { get; init; }
    public string? OrganizationNumber { get; init; }
    public string? Category { get; init; }
    public string? Urgency { get; init; }
    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid? AssignedToUserId { get; init; }
    public Guid? ConvertedCaseId { get; private set; }
}

