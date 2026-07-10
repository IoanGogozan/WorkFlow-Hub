namespace NorvixHub.Contracts.DemoStory;

public sealed record DemoStoryOutcomeResponse(
    string CaseNumber,
    string CaseTitle,
    string CustomerName,
    int LinkedDocumentCount,
    string DeliveryPackageTitle,
    string DeliveryPackageStatus,
    int AuditEventCount);
