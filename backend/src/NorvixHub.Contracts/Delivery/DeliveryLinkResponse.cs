namespace NorvixHub.Contracts.Delivery;

public sealed record DeliveryLinkResponse(
    Guid Id,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? RecipientEmail,
    string? Token);
