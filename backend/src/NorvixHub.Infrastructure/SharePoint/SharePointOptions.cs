namespace NorvixHub.Infrastructure.SharePoint;

public sealed class SharePointOptions
{
    public string Mode { get; set; } = "Simulated";
    public string SimulatedSiteId { get; set; } = "site-demo-service";
    public string SimulatedSiteName { get; set; } = "Service Operations Demo";
    public string SimulatedDriveId { get; set; } = "drive-shared-documents";
    public string SimulatedLibraryName { get; set; } = "Shared Documents";
    public string SimulatedPermission { get; set; } = "write";
    public bool SimulateThrottling { get; set; }
    public string MicrosoftTenantId { get; set; } = string.Empty;
    public string MicrosoftClientId { get; set; } = string.Empty;
    public string MicrosoftClientSecret { get; set; } = string.Empty;
    public string MicrosoftSiteId { get; set; } = string.Empty;
    public string MicrosoftDriveId { get; set; } = string.Empty;
}
