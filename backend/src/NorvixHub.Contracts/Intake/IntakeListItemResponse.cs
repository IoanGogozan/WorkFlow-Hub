namespace NorvixHub.Contracts.Intake;

public sealed record IntakeListItemResponse(
    Guid Id,
    string Source,
    string Status,
    string Subject,
    string? CustomerName,
    string? Category,
    string? Urgency,
    DateTimeOffset ReceivedAt,
    DateTimeOffset CreatedAt);

