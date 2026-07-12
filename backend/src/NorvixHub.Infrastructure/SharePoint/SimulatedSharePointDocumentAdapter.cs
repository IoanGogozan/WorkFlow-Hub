using Microsoft.Extensions.Options;
using NorvixHub.Application.SharePoint;

namespace NorvixHub.Infrastructure.SharePoint;

public sealed class SimulatedSharePointDocumentAdapter(IOptions<SharePointOptions> options) : ISharePointDocumentAdapter
{
    public string Mode => "Simulated";

    public SharePointAdapterStatus GetStatus()
    {
        var configuration = options.Value;
        return new SharePointAdapterStatus(
            Mode,
            true,
            true,
            configuration.SimulatedSiteId,
            configuration.SimulatedSiteName,
            configuration.SimulatedDriveId,
            configuration.SimulatedLibraryName,
            "Sites.Selected simulation",
            configuration.SimulatedPermission,
            "Local SharePoint/Microsoft Graph simulator. No Microsoft 365 tenant is connected.");
    }
}
