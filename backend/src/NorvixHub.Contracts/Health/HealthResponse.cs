namespace NorvixHub.Contracts.Health;

public sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset CheckedAt);

