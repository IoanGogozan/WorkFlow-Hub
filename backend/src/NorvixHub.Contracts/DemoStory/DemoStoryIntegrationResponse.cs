namespace NorvixHub.Contracts.DemoStory;

public sealed record DemoStoryIntegrationResponse(
    string Provider,
    string DisplayName,
    string Mode,
    string Status,
    string Explanation);
