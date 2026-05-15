namespace NorvixHub.Contracts.Intake;

public sealed record IntakeItemResponse(
    Guid Id,
    Guid TenantId,
    string Source,
    string Status,
    string Subject,
    string Body,
    string? CustomerName,
    string? OrganizationNumber,
    string? Category,
    string? Urgency,
    DateTimeOffset ReceivedAt,
    DateTimeOffset CreatedAt);

