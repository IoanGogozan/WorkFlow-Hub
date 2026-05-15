using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Documents;

public sealed class DocumentRecord : TenantScopedEntity
{
    public required string Title { get; init; }
    public DocumentStatus Status { get; private set; } = DocumentStatus.Uploaded;
    public string? DocumentType { get; private set; }
    public Guid? CurrentVersionId { get; private set; }
    public Guid? CaseId { get; private set; }
    public Guid? CustomerId { get; init; }
    public DateOnly? ExpiryDate { get; private set; }
    public Guid? ClassificationReviewedBy { get; private set; }
    public DateTimeOffset? ClassificationReviewedAt { get; private set; }

    public void SetCurrentVersion(Guid versionId, Guid? userId, DateTimeOffset now)
    {
        CurrentVersionId = versionId;
        MarkUpdated(userId, now);
    }

    public void MarkNeedsReview(Guid? userId, DateTimeOffset now)
    {
        Status = DocumentStatus.NeedsReview;
        MarkUpdated(userId, now);
    }

    public void ApproveClassification(
        string documentType,
        DateOnly? expiryDate,
        Guid reviewedBy,
        DateTimeOffset reviewedAt)
    {
        DocumentType = documentType.Trim();
        ExpiryDate = expiryDate;
        ClassificationReviewedBy = reviewedBy;
        ClassificationReviewedAt = reviewedAt;
        Status = DocumentStatus.Approved;
        MarkUpdated(reviewedBy, reviewedAt);
    }

    public void LinkToCase(Guid caseId, Guid? userId, DateTimeOffset now)
    {
        CaseId = caseId;
        MarkUpdated(userId, now);
    }
}

