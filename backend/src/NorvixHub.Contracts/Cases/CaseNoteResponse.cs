namespace NorvixHub.Contracts.Cases;

public sealed record CaseNoteResponse(
    Guid Id,
    Guid CaseId,
    string Body,
    string Visibility,
    DateTimeOffset CreatedAt);

