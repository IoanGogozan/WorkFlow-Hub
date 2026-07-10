namespace NorvixHub.Contracts.DemoStory;

public sealed record DemoStoryTechnicalLinksResponse(
    string IntakeHref,
    string CaseHref,
    string? PrimaryDocumentHref,
    string? DeliveryPackageHref,
    string IntegrationsHref);
