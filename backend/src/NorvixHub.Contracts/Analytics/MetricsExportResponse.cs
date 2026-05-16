namespace NorvixHub.Contracts.Analytics;

public sealed record MetricsExportResponse(
    MetricsOverviewResponse Overview,
    IReadOnlyCollection<MetricCountResponse> CasesByStatus,
    IReadOnlyCollection<MetricCountResponse> IntakesByStatus,
    IReadOnlyCollection<MetricCountResponse> DocumentsByStatus,
    IReadOnlyCollection<MetricCountResponse> IntegrationSyncsByStatus);
