using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Intake;

public sealed class IntakeItem : TenantScopedEntity
{
    public required IntakeSource Source { get; init; }
    public IntakeStatus Status { get; private set; } = IntakeStatus.New;
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? CustomerName { get; set; }
    public string? OrganizationNumber { get; set; }
    public string? Category { get; set; }
    public string? Urgency { get; set; }
    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid? AssignedToUserId { get; init; }
    public Guid? ConvertedCaseId { get; private set; }

    public void MarkAiNeedsReview(Guid? userId, DateTimeOffset now)
    {
        Status = IntakeStatus.NeedsReview;
        MarkUpdated(userId, now);
    }

    public void ApproveAiSuggestion(
        string? customerName,
        string? organizationNumber,
        string? category,
        string? urgency,
        Guid userId,
        DateTimeOffset now)
    {
        CustomerName = Normalize(customerName) ?? CustomerName;
        OrganizationNumber = Normalize(organizationNumber) ?? OrganizationNumber;
        Category = Normalize(category) ?? Category;
        Urgency = Normalize(urgency) ?? Urgency;
        Status = IntakeStatus.Approved;
        MarkUpdated(userId, now);
    }

    public void MarkConvertedToCase(Guid caseId, Guid userId, DateTimeOffset now)
    {
        ConvertedCaseId = caseId;
        Status = IntakeStatus.ConvertedToCase;
        MarkUpdated(userId, now);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
