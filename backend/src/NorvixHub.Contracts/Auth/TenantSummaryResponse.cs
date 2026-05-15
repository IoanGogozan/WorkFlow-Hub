namespace NorvixHub.Contracts.Auth;

public sealed record TenantSummaryResponse(
    Guid TenantId,
    string Name,
    string Slug,
    string Role);

