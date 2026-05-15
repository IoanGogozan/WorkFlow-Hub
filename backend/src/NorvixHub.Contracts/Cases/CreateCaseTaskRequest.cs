namespace NorvixHub.Contracts.Cases;

public sealed record CreateCaseTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate);

