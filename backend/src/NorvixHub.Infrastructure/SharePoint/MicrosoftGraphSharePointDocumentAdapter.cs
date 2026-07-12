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

    public Task<SharePointSyncResult> SynchronizeAsync(SharePointDocumentSyncRequest request, CancellationToken cancellationToken) => Task.FromResult(new SharePointSyncResult(false, false, 503, "NOT_CONFIGURED", "Microsoft Graph provider is not configured.", null));
    public Task<IReadOnlyList<SharePointDocumentItem>> ListCaseDocumentsAsync(Guid tenantId, Guid caseId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<SharePointDocumentItem>>([]);
    public Task<SharePointAccessResult> TestSiteAccessAsync(Guid tenantId, string siteId, CancellationToken cancellationToken) => Task.FromResult(new SharePointAccessResult(false, 503, "NOT_CONFIGURED", "Microsoft Graph provider is not configured."));
}
