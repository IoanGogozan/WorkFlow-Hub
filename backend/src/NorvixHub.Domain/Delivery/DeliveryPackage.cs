using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Delivery;

public sealed class DeliveryPackage : TenantScopedEntity
{
    public Guid CaseId { get; init; }
    public required string Title { get; init; }
    public DeliveryPackageStatus Status { get; private set; } = DeliveryPackageStatus.Draft;
    public Guid? SummaryPdfDocumentId { get; private set; }
    public DateTimeOffset? SummaryGeneratedAt { get; private set; }

    public void MarkSummaryGenerated(Guid? summaryDocumentId, Guid? userId, DateTimeOffset now)
    {
        SummaryPdfDocumentId = summaryDocumentId;
        SummaryGeneratedAt = now;
        Status = DeliveryPackageStatus.Ready;
        MarkUpdated(userId, now);
    }

    public void MarkDelivered(Guid? userId, DateTimeOffset now)
    {
        Status = DeliveryPackageStatus.Delivered;
        MarkUpdated(userId, now);
    }
}
