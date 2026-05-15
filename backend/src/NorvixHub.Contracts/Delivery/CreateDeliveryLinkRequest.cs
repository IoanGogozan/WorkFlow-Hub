namespace NorvixHub.Contracts.Delivery;

public sealed record CreateDeliveryLinkRequest(
    string? RecipientEmail,
    DateTimeOffset ExpiresAt);
