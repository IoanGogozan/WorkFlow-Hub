namespace NorvixHub.Contracts.Delivery;

public sealed record CreateDeliveryPackageRequest(
    string? Title,
    IReadOnlyCollection<Guid> DocumentIds);
