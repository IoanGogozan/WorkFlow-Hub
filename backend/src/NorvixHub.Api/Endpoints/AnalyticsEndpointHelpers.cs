using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Analytics;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.Integrations;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class AnalyticsEndpoints
{
    private static async Task<MetricsOverviewResponse> BuildOverviewAsync(
        Guid tenantId,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var newIntakes = await dbContext.IntakeItems.CountAsync(
            intake => intake.TenantId == tenantId && intake.Status == IntakeStatus.New,
            cancellationToken);
        var openCases = await dbContext.Cases.CountAsync(
            caseWorkspace => caseWorkspace.TenantId == tenantId,
            cancellationToken);
        var documentsNeedingReview = await dbContext.Documents.CountAsync(
            document => document.TenantId == tenantId && document.Status == DocumentStatus.NeedsReview,
            cancellationToken);
        var deliveryLinks = await dbContext.DeliveryLinks.CountAsync(
            link => link.TenantId == tenantId && link.RevokedAt == null && link.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);
        var integrationFailures = await dbContext.IntegrationSyncRuns.CountAsync(
            run => run.TenantId == tenantId && run.Status == IntegrationSyncStatus.Failed,
            cancellationToken);

        return new MetricsOverviewResponse(newIntakes, openCases, documentsNeedingReview, deliveryLinks, integrationFailures);
    }

    private static async Task<List<MetricCountResponse>> CountByStatusAsync<TStatus>(
        IQueryable<TStatus> query,
        CancellationToken cancellationToken)
        where TStatus : struct, Enum
    {
        var statuses = await query.ToListAsync(cancellationToken);
        return statuses
            .GroupBy(status => status)
            .Select(group => new MetricCountResponse(group.Key.ToString(), group.Count()))
            .OrderBy(item => item.Name)
            .ToList();
    }

    private static async Task<MetricsExportResponse> BuildExportAsync(
        Guid tenantId,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return new MetricsExportResponse(
            await BuildOverviewAsync(tenantId, dbContext, cancellationToken),
            await GetCaseStatusCountsAsync(tenantId, dbContext, cancellationToken),
            await GetIntakeStatusCountsAsync(tenantId, dbContext, cancellationToken),
            await GetDocumentStatusCountsAsync(tenantId, dbContext, cancellationToken),
            await GetIntegrationStatusCountsAsync(tenantId, dbContext, cancellationToken));
    }

    private static Task<List<MetricCountResponse>> GetCaseStatusCountsAsync(
        Guid tenantId,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return CountByStatusAsync(
            dbContext.Cases.Where(item => item.TenantId == tenantId).Select(item => item.Status),
            cancellationToken);
    }

    private static Task<List<MetricCountResponse>> GetIntakeStatusCountsAsync(
        Guid tenantId,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return CountByStatusAsync(
            dbContext.IntakeItems.Where(item => item.TenantId == tenantId).Select(item => item.Status),
            cancellationToken);
    }

    private static Task<List<MetricCountResponse>> GetDocumentStatusCountsAsync(
        Guid tenantId,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return CountByStatusAsync(
            dbContext.Documents.Where(item => item.TenantId == tenantId).Select(item => item.Status),
            cancellationToken);
    }

    private static Task<List<MetricCountResponse>> GetIntegrationStatusCountsAsync(
        Guid tenantId,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return CountByStatusAsync(
            dbContext.IntegrationSyncRuns.Where(item => item.TenantId == tenantId).Select(item => item.Status),
            cancellationToken);
    }

    private static Guid? GetTenantId(ITenantContext tenantContext)
    {
        return tenantContext.TenantId;
    }
}
