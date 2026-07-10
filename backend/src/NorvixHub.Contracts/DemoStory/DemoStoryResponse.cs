namespace NorvixHub.Contracts.DemoStory;

public sealed record DemoStoryResponse(
    string ScenarioKey,
    DemoStoryRequestResponse Request,
    DemoStoryOutcomeResponse Outcome,
    IReadOnlyList<DemoStoryEvidenceStepResponse> EvidenceSteps,
    IReadOnlyList<DemoStoryIntegrationResponse> Integrations,
    DemoStoryTechnicalLinksResponse TechnicalLinks);
