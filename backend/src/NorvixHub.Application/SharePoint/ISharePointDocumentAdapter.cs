namespace NorvixHub.Application.SharePoint;

public interface ISharePointDocumentAdapter
{
    string Mode { get; }

    SharePointAdapterStatus GetStatus();

    Task<SharePointSyncResult> SynchronizeAsync(SharePointDocumentSyncRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<SharePointDocumentItem>> ListCaseDocumentsAsync(Guid tenantId, Guid caseId, CancellationToken cancellationToken);

    Task<SharePointAccessResult> TestSiteAccessAsync(Guid tenantId, string siteId, CancellationToken cancellationToken);
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

public sealed record SharePointDocumentSyncRequest(
    Guid TenantId,
    Guid? ActorId,
    Guid CaseId,
    string CustomerName,
    string CaseNumber,
    Guid DocumentId,
    Guid DocumentVersionId,
    string Filename,
    long SizeBytes,
    string DocumentType,
    string Status,
    string? ExpectedETag = null,
    Guid? IntegrationSyncRunId = null,
    Guid? LiveDemoRunId = null);

public sealed record SharePointSyncResult(
    bool Succeeded,
    bool AlreadySynchronized,
    int StatusCode,
    string? ErrorCode,
    string? PublicMessage,
    SharePointDocumentItem? Item);

public sealed record SharePointDocumentItem(
    string SiteId,
    string DriveId,
    string ExternalItemId,
    string ParentPath,
    string Name,
    string ETag,
    string Version,
    long SizeBytes,
    string DocumentType,
    string Status);

public sealed record SharePointAccessResult(bool Succeeded, int StatusCode, string? ErrorCode, string PublicMessage);
