using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Reviews;

public sealed class ReviewTask : TenantScopedEntity
{
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public required string ReviewType { get; init; }
    public ReviewTaskStatus Status { get; private set; } = ReviewTaskStatus.Pending;
    public Guid? AssignedToUserId { get; init; }
    public Guid? AiAnalysisRunId { get; init; }
    public string? DecisionJson { get; private set; }
    public Guid? DecidedBy { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }

    public void MarkApproved(Guid decidedBy, string decisionJson, DateTimeOffset decidedAt)
    {
        Status = ReviewTaskStatus.Approved;
        DecisionJson = decisionJson;
        DecidedBy = decidedBy;
        DecidedAt = decidedAt;
        MarkUpdated(decidedBy, decidedAt);
    }

    public void MarkRejected(Guid decidedBy, string decisionJson, DateTimeOffset decidedAt)
    {
        Status = ReviewTaskStatus.Rejected;
        DecisionJson = decisionJson;
        DecidedBy = decidedBy;
        DecidedAt = decidedAt;
        MarkUpdated(decidedBy, decidedAt);
    }
}

