using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.LiveDemoEvidence;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.Infrastructure.SharePoint;

namespace NorvixHub.Api.Endpoints;

public static class LiveDemoEvidenceEndpoints
{
    public static IEndpointRouteBuilder MapLiveDemoEvidenceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/live-demo-runs/{runId:guid}/evidence", GetEvidence)
            .WithName("GetLiveDemoEvidence");
        return app;
    }

    private static async Task<IResult> GetEvidence(
        Guid runId,
        ITenantContext tenantContext,
        IOptions<SharePointOptions> sharePointOptions,
        NorvixHubDbContext db,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var run = await db.LiveDemoRuns.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.Id == runId && candidate.TenantId == tenantId,
            cancellationToken);
        if (run is null)
        {
            return Results.NotFound(new { error = "Live demo run was not found." });
        }

        var intake = run.IntakeItemId is { } intakeId
            ? await db.IntakeItems.AsNoTracking().SingleOrDefaultAsync(
                candidate => candidate.Id == intakeId && candidate.TenantId == tenantId,
                cancellationToken)
            : null;
        var customer = run.CustomerId is { } customerId
            ? await db.Customers.AsNoTracking().SingleOrDefaultAsync(
                candidate => candidate.Id == customerId && candidate.TenantId == tenantId,
                cancellationToken)
            : null;
        var caseItem = run.CaseId is { } caseId
            ? await db.Cases.AsNoTracking().SingleOrDefaultAsync(
                candidate => candidate.Id == caseId && candidate.TenantId == tenantId,
                cancellationToken)
            : null;
        var document = run.DocumentId is { } documentId
            ? await db.Documents.AsNoTracking().SingleOrDefaultAsync(
                candidate => candidate.Id == documentId && candidate.TenantId == tenantId,
                cancellationToken)
            : null;
        var version = document?.CurrentVersionId is { } versionId
            ? await db.DocumentVersions.AsNoTracking().SingleOrDefaultAsync(
                candidate => candidate.Id == versionId &&
                    candidate.DocumentId == document.Id &&
                    candidate.TenantId == tenantId,
                cancellationToken)
            : null;
        var brregStep = await db.LiveDemoRunSteps.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.RunId == run.Id &&
                candidate.TenantId == tenantId &&
                candidate.Key == "brreg-checked",
            cancellationToken);

        var sharePointItem = run.DocumentId is { } synchronizedDocumentId
            ? await db.SimulatedSharePointDocumentItems.AsNoTracking().SingleOrDefaultAsync(
                candidate => candidate.TenantId == tenantId &&
                    candidate.DocumentId == synchronizedDocumentId,
                cancellationToken)
            : null;
        var sharePointOperations = await db.SimulatedSharePointOperations.AsNoTracking()
            .Where(operation => operation.TenantId == tenantId && operation.LiveDemoRunId == run.Id)
            .OrderBy(operation => operation.CreatedAt)
            .ThenBy(operation => operation.Id)
            .ToListAsync(cancellationToken);

        var relatedEntityIds = new[]
        {
            run.Id.ToString(), run.IntakeItemId?.ToString(), run.CustomerId?.ToString(),
            run.CaseId?.ToString(), run.DocumentId?.ToString(), run.DeliveryPackageId?.ToString()
        }.Where(value => value is not null).Select(value => value!).ToArray();
        var auditEvents = await db.AuditEvents.AsNoTracking()
            .Where(audit => audit.TenantId == tenantId &&
                (relatedEntityIds.Contains(audit.EntityId) || audit.CorrelationId == run.CorrelationId))
            .OrderBy(audit => audit.CreatedAt)
            .ThenBy(audit => audit.Id)
            .ToListAsync(cancellationToken);

        return Results.Ok(CreateResponse(
            run, intake, customer, caseItem, document, version, brregStep,
            sharePointItem, sharePointOperations, auditEvents, sharePointOptions.Value));
    }

    private static LiveDemoEvidenceResponse CreateResponse(
        LiveDemoRun run,
        Domain.Intake.IntakeItem? intake,
        Domain.Customers.Customer? customer,
        Domain.Cases.CaseWorkspace? caseItem,
        Domain.Documents.DocumentRecord? document,
        Domain.Documents.DocumentVersion? version,
        LiveDemoRunStep? brregStep,
        Domain.SharePoint.SimulatedSharePointDocumentItem? sharePointItem,
        IReadOnlyList<Domain.SharePoint.SimulatedSharePointOperation> sharePointOperations,
        IReadOnlyList<Domain.Audit.AuditEvent> auditEvents,
        SharePointOptions sharePointOptions)
    {
        var caseHref = caseItem is null ? null : $"/cases/{caseItem.Id}";
        var documentHref = document is null ? null : $"/documents/{document.Id}";
        var downloadHref = document is null ? null : $"/api/documents/{document.Id}/download";
        var deliveryHref = run.DeliveryPackageId is { } packageId ? $"/delivery-packages/{packageId}" : null;

        return new LiveDemoEvidenceResponse(
            new LiveDemoEvidenceRunResponse(
                run.Id, run.Status.ToString(), Shorten(run.CorrelationId)!, run.CreatedAt,
                run.StartedAt, run.CompletedAt, run.TotalDurationMs, run.RetryCount,
                "Fiktive data — servicehenvendelse"),
            new LiveDemoEvidenceRequestResponse(
                intake?.Subject ?? run.RequestTitle,
                intake?.Body ?? run.RequestBody,
                run.CustomerReference,
                "Fiktiv henvendelse",
                intake?.CreatedAt ?? run.CreatedAt),
            CreateBrreg(run, customer, brregStep),
            caseItem is null
                ? null
                : new LiveDemoEvidenceCaseResponse(
                    caseItem.CaseNumber, caseItem.Title, caseItem.Status.ToString(),
                    customer?.Name ?? "Ukjent kunde", caseItem.CreatedAt, caseHref!),
            document is null || version is null
                ? null
                : new LiveDemoEvidenceDocumentResponse(
                    document.Id, document.Title, version.OriginalFilename, version.SizeBytes,
                    version.ContentType, version.VersionNumber, Shorten(version.Sha256Hash),
                    document.CreatedAt, documentHref!, downloadHref!),
            CreateSharePoint(run, sharePointItem, sharePointOperations, sharePointOptions),
            run.ErpReceiptId is null
                ? null
                : new LiveDemoEvidenceErpResponse(
                    "demo-receiver", "Received", Shorten(run.ErpReceiptId), null, 1, null, null),
            auditEvents.Select(audit => new LiveDemoEvidenceAuditEventResponse(
                audit.CreatedAt, audit.Action, audit.Action, audit.EntityType,
                "Recorded", Shorten(audit.CorrelationId ?? run.CorrelationId)!)).ToList(),
            new LiveDemoEvidenceLinksResponse(
                caseHref, documentHref, downloadHref, deliveryHref,
                "/technical/sharepoint", "/integrations"));
    }

    private static LiveDemoEvidenceBrregResponse? CreateBrreg(
        LiveDemoRun run,
        Domain.Customers.Customer? customer,
        LiveDemoRunStep? step)
    {
        if (string.IsNullOrWhiteSpace(run.BrregMode))
        {
            return null;
        }

        var mode = run.BrregMode.ToLowerInvariant();
        return new LiveDemoEvidenceBrregResponse(
            mode,
            run.OrganizationNumber,
            customer?.Name ?? "Organisasjon ikke tilgjengelig",
            step?.DurationMs,
            run.BrregSourceUpdatedAt ?? customer?.SourceUpdatedAt,
            mode == "live"
                ? "Kontrollert mot Brreg."
                : "Kontrollert med tydelig merket fallback-snapshot.");
    }

    private static LiveDemoEvidenceSharePointResponse? CreateSharePoint(
        LiveDemoRun run,
        Domain.SharePoint.SimulatedSharePointDocumentItem? item,
        IReadOnlyList<Domain.SharePoint.SimulatedSharePointOperation> operations,
        SharePointOptions options)
    {
        if (item is null || run.SharePointFolderItemId is null || run.SharePointFileItemId is null)
        {
            return null;
        }

        var mappedOperations = operations.Select(operation =>
            new LiveDemoEvidenceSharePointOperationResponse(
                operation.CreatedAt,
                operation.HttpMethod,
                operation.Target,
                operation.StatusCode,
                operation.Succeeded ? "Succeeded" : "Failed",
                operation.DurationMilliseconds,
                1,
                operation.Operation == "UploadDocument" && operation.StatusCode == StatusCodes.Status200OK
                    ? "reused"
                    : "recorded")).ToList();

        return new LiveDemoEvidenceSharePointResponse(
            "simulated",
            options.SimulatedSiteName,
            options.SimulatedLibraryName,
            item.ParentPath,
            Shorten(run.SharePointFolderItemId)!,
            Shorten(run.SharePointFileItemId)!,
            item.Name,
            int.TryParse(item.Version, out var version) ? version : 1,
            item.ETag,
            ParseMetadata(item.MetadataJson),
            mappedOperations,
            "/technical/sharepoint");
    }

    private static IReadOnlyDictionary<string, string> ParseMetadata(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ??
                new Dictionary<string, string>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }

    private static string? Shorten(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Length <= 16
            ? value
            : $"{value[..8]}…{value[^6..]}";
}
