namespace NorvixHub.Domain.AI;

public sealed class AiAnalysisRun
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public required string Provider { get; init; }
    public required string Model { get; init; }
    public required string PromptVersion { get; init; }
    public required string InputHash { get; init; }
    public required string OutputJson { get; init; }
    public decimal Confidence { get; init; }
    public AiAnalysisStatus Status { get; private set; } = AiAnalysisStatus.NeedsReview;
    public Guid? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public void MarkApproved(Guid reviewedBy, DateTimeOffset reviewedAt)
    {
        Status = AiAnalysisStatus.Approved;
        ReviewedBy = reviewedBy;
        ReviewedAt = reviewedAt;
    }

    public void MarkRejected(Guid reviewedBy, DateTimeOffset reviewedAt)
    {
        Status = AiAnalysisStatus.Rejected;
        ReviewedBy = reviewedBy;
        ReviewedAt = reviewedAt;
    }
}

