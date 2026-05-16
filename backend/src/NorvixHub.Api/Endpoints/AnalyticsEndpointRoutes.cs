namespace NorvixHub.Api.Endpoints;

public static partial class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/metrics");

        group.MapGet("/overview", GetOverview).WithName("GetMetricsOverview");
        group.MapGet("/cases", GetCaseMetrics).WithName("GetCaseMetrics");
        group.MapGet("/intakes", GetIntakeMetrics).WithName("GetIntakeMetrics");
        group.MapGet("/documents", GetDocumentMetrics).WithName("GetDocumentMetrics");
        group.MapGet("/integrations", GetIntegrationMetrics).WithName("GetIntegrationMetrics");
        group.MapGet("/export.json", ExportJson).WithName("ExportMetricsJson");
        group.MapGet("/export.csv", ExportCsv).WithName("ExportMetricsCsv");

        return app;
    }
}
