namespace NorvixHub.Contracts.AI;

public sealed record AiIntakeSuggestionResponse(
    string? CustomerName,
    string? OrganizationNumber,
    string? Category,
    string? Urgency,
    IReadOnlyList<string> SuggestedTasks,
    string Summary,
    IReadOnlyList<string> MissingInformation,
    decimal Confidence);

