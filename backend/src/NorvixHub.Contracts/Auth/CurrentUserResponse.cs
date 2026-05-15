namespace NorvixHub.Contracts.Auth;

public sealed record CurrentUserResponse(
    Guid UserId,
    Guid TenantId,
    string DisplayName,
    string Email,
    string Role);

