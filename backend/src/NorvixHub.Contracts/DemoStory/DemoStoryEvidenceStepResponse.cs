namespace NorvixHub.Contracts.DemoStory;

public sealed record DemoStoryEvidenceStepResponse(
    string Key,
    int Sequence,
    string Title,
    string Description,
    string System,
    string EvidenceMode,
    string EvidenceLabel,
    string? EvidenceHref);
