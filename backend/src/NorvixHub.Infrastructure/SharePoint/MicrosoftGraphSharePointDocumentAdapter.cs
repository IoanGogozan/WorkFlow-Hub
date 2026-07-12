using Microsoft.Extensions.Options;
using NorvixHub.Application.SharePoint;

namespace NorvixHub.Infrastructure.SharePoint;

public sealed class MicrosoftGraphSharePointDocumentAdapter(IOptions<SharePointOptions> options) : ISharePointDocumentAdapter
{
    public string Mode => "MicrosoftGraph";

    public SharePointAdapterStatus GetStatus()
    {
        var configuration = options.Value;
        var configured = HasValue(configuration.MicrosoftTenantId) &&
            HasValue(configuration.MicrosoftClientId) &&
            HasValue(configuration.MicrosoftClientSecret) &&
            HasValue(configuration.MicrosoftSiteId) &&
            HasValue(configuration.MicrosoftDriveId);
        return new SharePointAdapterStatus(
            Mode,
            false,
            configured,
            configured ? configuration.MicrosoftSiteId : string.Empty,
            "Microsoft Graph site",
            configured ? configuration.MicrosoftDriveId : string.Empty,
            "Microsoft Graph document library",
            "Sites.Selected",
            "write",
            configured
                ? "Microsoft Graph provider is configured but live calls are not enabled."
                : "Microsoft Graph provider is not configured.");
    }

    private static bool HasValue(string value) => !string.IsNullOrWhiteSpace(value);
}
