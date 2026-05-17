namespace NorvixHub.Contracts.Auth;

public sealed record CreateDemoSessionResponse(
    Guid SessionId,
    Guid DemoTenantId,
    string Token,
    DateTimeOffset ExpiresAt);
