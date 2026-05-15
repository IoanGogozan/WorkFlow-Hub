namespace NorvixHub.Contracts.Delivery;

public sealed record PublicDeliveryDocumentResponse(
    Guid DocumentId,
    string Title,
    string? DocumentType);
