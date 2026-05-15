namespace NorvixHub.Application.Audit;

public sealed record AuditEventRequest(
    Guid TenantId,
    Guid? ActorUserId,
    string ActorType,
    string EntityType,
    string EntityId,
    string Action,
    string? BeforeJson,
    string? AfterJson,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId);

