namespace NorvixHub.Contracts.Delivery;

public sealed record DeliveryPackageItemResponse(
    Guid Id,
    Guid DocumentId,
    string DisplayName);
