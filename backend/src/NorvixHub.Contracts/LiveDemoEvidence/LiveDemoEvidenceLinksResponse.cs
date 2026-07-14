namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceLinksResponse(
    string? CaseHref,
    string? DocumentHref,
    string? DownloadHref,
    string? DeliveryPackageHref,
    string SharePointTechnicalHref,
    string IntegrationDashboardHref);
