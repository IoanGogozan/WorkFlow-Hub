using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Analytics;
using NorvixHub.Contracts.Intake;
using NorvixHub.Contracts.Integrations;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Analytics;

public sealed class AnalyticsEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public AnalyticsEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Overview_returns_tenant_scoped_operational_metrics()
    {
        await _factory.SeedExtraTenantsAsync();
        await _factory.CreateSecondTenantIntakeAsync();
        using var client = _factory.CreateClient();
        await CreateIntakeAsync(client);
        await CreateFailedIntegrationSyncAsync(client);

        var overview = await GetJsonAsync<MetricsOverviewResponse>(client, "/api/metrics/overview");

        overview.NewIntakes.Should().BeGreaterThanOrEqualTo(1);
        overview.IntegrationFailures.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Status_metric_endpoints_return_grouped_counts()
    {
        using var client = _factory.CreateClient();
        await CreateIntakeAsync(client);

        var intakes = await GetJsonAsync<List<MetricCountResponse>>(client, "/api/metrics/intakes");
        var cases = await GetJsonAsync<List<MetricCountResponse>>(client, "/api/metrics/cases");
        var documents = await GetJsonAsync<List<MetricCountResponse>>(client, "/api/metrics/documents");
        var integrations = await GetJsonAsync<List<MetricCountResponse>>(client, "/api/metrics/integrations");

        intakes.Should().Contain(item => item.Name == "New");
        cases.Should().NotBeNull();
        documents.Should().NotBeNull();
        integrations.Should().NotBeNull();
    }

    [Fact]
    public async Task Json_export_contains_overview_and_status_sections()
    {
        using var client = _factory.CreateClient();
        await CreateIntakeAsync(client);

        var export = await GetJsonAsync<MetricsExportResponse>(client, "/api/metrics/export.json");

        export.Overview.NewIntakes.Should().BeGreaterThanOrEqualTo(1);
        export.IntakesByStatus.Should().Contain(item => item.Name == "New");
    }

    [Fact]
    public async Task Csv_export_contains_expected_sections()
    {
        using var client = _factory.CreateClient();
        await CreateIntakeAsync(client);
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Get, "/api/metrics/export.csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var csv = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        csv.Should().Contain("section,name,count");
        csv.Should().Contain("overview,new_intakes");
        csv.Should().Contain("intakes,New");
    }

    [Fact]
    public async Task Export_without_auth_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/metrics/export.json", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Viewer_can_read_metrics_but_only_for_own_tenant()
    {
        await _factory.SeedExtraTenantsAsync();
        await _factory.CreateSecondTenantIntakeAsync();
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/metrics/export.json");
        DevAuthHeaders.Add(request, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var export = await response.Content.ReadFromJsonAsync<MetricsExportResponse>(
            TestContext.Current.CancellationToken);
        export!.Overview.NewIntakes.Should().BeGreaterThanOrEqualTo(0);
    }

    private static async Task CreateIntakeAsync(HttpClient client)
    {
        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            "/api/intakes",
            new CreateIntakeRequest("Manual", $"Metric intake {Guid.NewGuid():N}", "Analytics test.", null, null, null, "Normal"));
        response.EnsureSuccessStatusCode();
    }

    private static async Task CreateFailedIntegrationSyncAsync(HttpClient client)
    {
        using var connectResponse = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            "/api/integrations/tripletex/connect",
            new ConnectIntegrationRequest("{\"forceFailure\":true}"));
        connectResponse.EnsureSuccessStatusCode();

        using var syncResponse = await SendWithDemoAuthAsync(client, HttpMethod.Post, "/api/integrations/tripletex/sync");
        syncResponse.EnsureSuccessStatusCode();
    }

    private static async Task<T> GetJsonAsync<T>(HttpClient client, string url)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(
            TestContext.Current.CancellationToken))!;
    }

    private static Task<HttpResponseMessage> SendWithDemoAuthAsync(
        HttpClient client,
        HttpMethod method,
        string url,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        DevAuthHeaders.AddDemoAdmin(request);
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
