namespace NorvixHub.Contracts.Intake;

public sealed record CreateIntakeRequest(
    string Source,
    string Subject,
    string Body,
    string? CustomerName,
    string? OrganizationNumber,
    string? Category,
    string? Urgency);

