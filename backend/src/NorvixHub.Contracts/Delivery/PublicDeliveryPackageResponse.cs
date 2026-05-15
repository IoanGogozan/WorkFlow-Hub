namespace NorvixHub.Contracts.Delivery;

public sealed record PublicDeliveryPackageResponse(
    string Title,
    string CaseTitle,
    DateTimeOffset ExpiresAt,
    IReadOnlyCollection<PublicDeliveryDocumentResponse> Documents);
