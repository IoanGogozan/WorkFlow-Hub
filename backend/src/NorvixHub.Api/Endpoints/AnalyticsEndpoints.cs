using System.Text;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Analytics;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class AnalyticsEndpoints
{
    private static async Task<IResult> GetOverview(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return GetTenantId(tenantContext) is { } tenantId
            ? Results.Ok(await BuildOverviewAsync(tenantId, dbContext, cancellationToken))
            : Results.Unauthorized();
    }

    private static async Task<IResult> GetCaseMetrics(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return GetTenantId(tenantContext) is { } tenantId
            ? Results.Ok(await GetCaseStatusCountsAsync(tenantId, dbContext, cancellationToken))
            : Results.Unauthorized();
    }

    private static async Task<IResult> GetIntakeMetrics(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return GetTenantId(tenantContext) is { } tenantId
            ? Results.Ok(await GetIntakeStatusCountsAsync(tenantId, dbContext, cancellationToken))
            : Results.Unauthorized();
    }

    private static async Task<IResult> GetDocumentMetrics(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return GetTenantId(tenantContext) is { } tenantId
            ? Results.Ok(await GetDocumentStatusCountsAsync(tenantId, dbContext, cancellationToken))
            : Results.Unauthorized();
    }

    private static async Task<IResult> GetIntegrationMetrics(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return GetTenantId(tenantContext) is { } tenantId
            ? Results.Ok(await GetIntegrationStatusCountsAsync(tenantId, dbContext, cancellationToken))
            : Results.Unauthorized();
    }

    private static async Task<IResult> ExportJson(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return GetTenantId(tenantContext) is { } tenantId
            ? Results.Ok(await BuildExportAsync(tenantId, dbContext, cancellationToken))
            : Results.Unauthorized();
    }

    private static async Task<IResult> ExportCsv(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (GetTenantId(tenantContext) is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var export = await BuildExportAsync(tenantId, dbContext, cancellationToken);
        var csv = BuildCsv(export);
        return Results.Text(csv, "text/csv", Encoding.UTF8);
    }

    private static string BuildCsv(MetricsExportResponse export)
    {
        var builder = new StringBuilder();
        builder.AppendLine("section,name,count");
        AppendOverview(builder, export.Overview);
        AppendRows(builder, "cases", export.CasesByStatus);
        AppendRows(builder, "intakes", export.IntakesByStatus);
        AppendRows(builder, "documents", export.DocumentsByStatus);
        AppendRows(builder, "integrations", export.IntegrationSyncsByStatus);
        return builder.ToString();
    }

    private static void AppendOverview(StringBuilder builder, MetricsOverviewResponse overview)
    {
        builder.AppendLine($"overview,new_intakes,{overview.NewIntakes}");
        builder.AppendLine($"overview,open_cases,{overview.OpenCases}");
        builder.AppendLine($"overview,documents_needing_review,{overview.DocumentsNeedingReview}");
        builder.AppendLine($"overview,delivery_links,{overview.DeliveryLinks}");
        builder.AppendLine($"overview,integration_failures,{overview.IntegrationFailures}");
    }

    private static void AppendRows(StringBuilder builder, string section, IReadOnlyCollection<MetricCountResponse> rows)
    {
        foreach (var row in rows)
        {
            builder.AppendLine($"{section},{row.Name},{row.Count}");
        }
    }
}
