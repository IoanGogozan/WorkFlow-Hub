namespace NorvixHub.Contracts.DemoStory;

public sealed record DemoStoryRequestResponse(
    string Source,
    string Sender,
    string Subject,
    string Body,
    string CustomerName,
    string? OrganizationNumber,
    string? CustomerReference,
    IReadOnlyList<string> Attachments,
    DateTimeOffset ReceivedAt);
