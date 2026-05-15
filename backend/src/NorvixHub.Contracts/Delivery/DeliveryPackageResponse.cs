namespace NorvixHub.Contracts.Delivery;

public sealed record DeliveryPackageResponse(
    Guid Id,
    Guid CaseId,
    string Title,
    string Status,
    Guid? SummaryPdfDocumentId,
    DateTimeOffset? SummaryGeneratedAt,
    IReadOnlyCollection<DeliveryPackageItemResponse> Items,
    IReadOnlyCollection<DeliveryLinkResponse> Links);
