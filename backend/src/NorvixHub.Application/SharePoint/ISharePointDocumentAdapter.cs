namespace NorvixHub.Application.SharePoint;

public interface ISharePointDocumentAdapter
{
    string Mode { get; }

    SharePointAdapterStatus GetStatus();
}

public interface ISharePointDocumentAdapterResolver
{
    ISharePointDocumentAdapter GetCurrent();
}

public sealed record SharePointAdapterStatus(
    string Mode,
    bool IsSimulated,
    bool IsConfigured,
    string SiteId,
    string SiteName,
    string DriveId,
    string LibraryName,
    string PermissionModel,
    string PermissionLevel,
    string PublicMessage);
