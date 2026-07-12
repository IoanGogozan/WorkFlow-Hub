using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.SharePoint;
using NorvixHub.Domain.SharePoint;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Infrastructure.SharePoint;

public sealed class SimulatedSharePointDocumentAdapter(
    NorvixHubDbContext dbContext,
    IOptions<SharePointOptions> options) : ISharePointDocumentAdapter
{
    public SimulatedSharePointDocumentAdapter(IOptions<SharePointOptions> options)
        : this(null!, options)
    {
    }

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

    public async Task<SharePointSyncResult> SynchronizeAsync(SharePointDocumentSyncRequest request, CancellationToken cancellationToken)
    {
        var configuration = options.Value;
        var key = string.Concat(request.TenantId.ToString("N"), ":", request.CaseId.ToString("N"), ":", request.DocumentId.ToString("N"), ":", request.DocumentVersionId.ToString("N"));
        var existing = await dbContext.SimulatedSharePointDocumentItems.SingleOrDefaultAsync(item => item.TenantId == request.TenantId && item.IdempotencyKey == key, cancellationToken);
        if (existing is not null)
        {
            await RecordAsync(request, "UploadDocument", "PUT", existing.ParentPath, 200, true, null, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new SharePointSyncResult(true, true, 200, null, "Already synchronized — no duplicate created.", ToContract(existing, request));
        }

        var parentPath = string.Concat("/", configuration.SimulatedLibraryName, "/Customers/", Sanitize(request.CustomerName), "/", Sanitize(request.CaseNumber), "/Incoming");
        foreach (var folder in new[] { "/" + configuration.SimulatedLibraryName, "/" + configuration.SimulatedLibraryName + "/Customers", parentPath })
        {
            await RecordAsync(request, "CreateFolder", "POST", folder, 201, true, null, cancellationToken);
        }

        var item = new SimulatedSharePointDocumentItem
        {
            TenantId = request.TenantId, CreatedBy = request.ActorId, SiteId = configuration.SimulatedSiteId, DriveId = configuration.SimulatedDriveId,
            DocumentId = request.DocumentId, DocumentVersionId = request.DocumentVersionId, CaseId = request.CaseId,
            ExternalItemId = "01SP-DEMO-" + request.DocumentId.ToString("N")[..10].ToUpperInvariant(), ParentPath = parentPath,
            Name = Sanitize(request.Filename), ETag = "demo-etag-1", Version = "1.0",
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { request.CaseNumber, Customer = request.CustomerName, request.DocumentType, request.Status }),
            SyncStatus = "Synchronized", IdempotencyKey = key, LastSyncedAt = DateTimeOffset.UtcNow
        };
        dbContext.SimulatedSharePointDocumentItems.Add(item);
        await RecordAsync(request, "UploadDocument", "PUT", parentPath + "/" + item.Name, 201, true, null, cancellationToken);
        await RecordAsync(request, "UpdateMetadata", "PATCH", parentPath + "/" + item.Name, 200, true, null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new SharePointSyncResult(true, false, 201, null, "Simulated document synchronized.", ToContract(item, request));
    }

    public async Task<IReadOnlyList<SharePointDocumentItem>> ListCaseDocumentsAsync(Guid tenantId, Guid caseId, CancellationToken cancellationToken) =>
        await dbContext.SimulatedSharePointDocumentItems.AsNoTracking().Where(item => item.TenantId == tenantId && item.CaseId == caseId)
            .Select(item => new SharePointDocumentItem(item.SiteId, item.DriveId, item.ExternalItemId, item.ParentPath, item.Name, item.ETag, item.Version, 0, "Document", item.SyncStatus)).ToListAsync(cancellationToken);

    public async Task<SharePointAccessResult> TestSiteAccessAsync(Guid tenantId, string siteId, CancellationToken cancellationToken) =>
        await Task.FromResult(new SharePointAccessResult(siteId == options.Value.SimulatedSiteId, siteId == options.Value.SimulatedSiteId ? 200 : 403, siteId == options.Value.SimulatedSiteId ? null : "accessDenied", "Access check is implemented in S2.1c."));

    private Task RecordAsync(SharePointDocumentSyncRequest request, string operation, string method, string target, int status, bool succeeded, string? error, CancellationToken cancellationToken)
    {
        dbContext.SimulatedSharePointOperations.Add(new SimulatedSharePointOperation { TenantId = request.TenantId, CreatedBy = request.ActorId, DocumentId = request.DocumentId, DocumentVersionId = request.DocumentVersionId, Operation = operation, HttpMethod = method, Target = target, StatusCode = status, Succeeded = succeeded, DurationMilliseconds = 0, ErrorCode = error });
        return Task.CompletedTask;
    }

    private static SharePointDocumentItem ToContract(SimulatedSharePointDocumentItem item, SharePointDocumentSyncRequest request) => new(item.SiteId, item.DriveId, item.ExternalItemId, item.ParentPath, item.Name, item.ETag, item.Version, request.SizeBytes, request.DocumentType, request.Status);
    private static string Sanitize(string value) => string.Concat(value.Select(character => "~#%&*{}\\:<>?/+|\"".Contains(character) ? '-' : character)).Trim(' ', '.');
}
