namespace NorvixHub.Contracts.Analytics;

public sealed record MetricsOverviewResponse(
    int NewIntakes,
    int OpenCases,
    int DocumentsNeedingReview,
    int DeliveryLinks,
    int IntegrationFailures);
