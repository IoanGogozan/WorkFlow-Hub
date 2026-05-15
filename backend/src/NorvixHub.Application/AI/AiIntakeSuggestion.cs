namespace NorvixHub.Application.AI;

public sealed record AiIntakeSuggestion(
    string? CustomerName,
    string? OrganizationNumber,
    string? Category,
    string? Urgency,
    IReadOnlyList<string> SuggestedTasks,
    string Summary,
    IReadOnlyList<string> MissingInformation,
    decimal Confidence);

