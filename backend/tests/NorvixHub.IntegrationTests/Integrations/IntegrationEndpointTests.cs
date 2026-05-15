using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Integrations;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Integrations;

public sealed class IntegrationEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public IntegrationEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task List_integrations_returns_default_mock_connectors()
    {
        using var client = _factory.CreateClient();

        var integrations = await GetJsonAsync<List<IntegrationConnectionResponse>>(client, "/api/integrations");

        integrations.Should().Contain(connection => connection.Provider == "tripletex");
        integrations.Should().Contain(connection => connection.Provider == "microsoft-graph");
        integrations.Should().Contain(connection => connection.Provider == "powerbi-fabric");
        integrations.Should().Contain(connection => connection.Provider == "brreg");
    }

    [Fact]
    public async Task Admin_can_connect_and_sync_tripletex()
    {
        using var client = _factory.CreateClient();
        var auditBefore = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "IntegrationConnection",
            "IntegrationSyncRunCreated");

        var connected = await ConnectAsync(client, "tripletex", "{}");
        using var syncResponse = await SendWithDemoAuthAsync(client, HttpMethod.Post, "/api/integrations/tripletex/sync");

        syncResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        connected.Status.Should().Be("Connected");
        var sync = await syncResponse.Content.ReadFromJsonAsync<IntegrationSyncRunResponse>(
            TestContext.Current.CancellationToken);
        sync!.Status.Should().Be("Succeeded");
        sync.ItemsProcessed.Should().BeGreaterThan(0);

        var auditAfter = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "IntegrationConnection",
            "IntegrationSyncRunCreated");
        auditAfter.Should().Be(auditBefore + 1);
    }

    [Fact]
    public async Task Failed_sync_can_be_retried_after_settings_are_fixed()
    {
        using var client = _factory.CreateClient();
        await ConnectAsync(client, "tripletex", "{\"forceFailure\":true}");
        var failed = await SyncAsync(client, "tripletex");
        failed.Status.Should().Be("Failed");

        await ConnectAsync(client, "tripletex", "{}");
        using var retryResponse = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/integrations/tripletex/sync-runs/{failed.Id}/retry");

        retryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retried = await retryResponse.Content.ReadFromJsonAsync<IntegrationSyncRunResponse>(
            TestContext.Current.CancellationToken);
        retried!.Status.Should().Be("Succeeded");
        retried.TriggeredBy.Should().Be("Retry");
        retried.RetriedFromSyncRunId.Should().Be(failed.Id);

        var runs = await GetJsonAsync<List<IntegrationSyncRunResponse>>(client, "/api/integrations/tripletex/sync-runs");
        runs.Should().Contain(run => run.Id == failed.Id && run.Status == "Failed");
        runs.Should().Contain(run => run.Id == retried.Id && run.Status == "Succeeded");
    }

    [Fact]
    public async Task Disconnected_integration_cannot_be_synced()
    {
        using var client = _factory.CreateClient();
        await SendWithDemoAuthAsync(client, HttpMethod.Post, "/api/integrations/tripletex/disconnect");

        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, "/api/integrations/tripletex/sync");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Viewer_cannot_edit_or_sync_integrations()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var connectRequest = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/tripletex/connect")
        {
            Content = JsonContent.Create(new ConnectIntegrationRequest("{}"))
        };
        DevAuthHeaders.Add(connectRequest, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(connectRequest, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Retry_sync_run_from_another_tenant_returns_not_found()
    {
        await _factory.SeedExtraTenantsAsync();
        var otherTenantRunId = await _factory.CreateSecondTenantFailedSyncRunAsync();
        using var client = _factory.CreateClient();
        await ConnectAsync(client, "tripletex", "{}");

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/integrations/tripletex/sync-runs/{otherTenantRunId}/retry");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unknown_provider_returns_not_found()
    {
        using var client = _factory.CreateClient();

        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Get, "/api/integrations/unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<IntegrationConnectionResponse> ConnectAsync(
        HttpClient client,
        string provider,
        string settingsJson)
    {
        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/integrations/{provider}/connect",
            new ConnectIntegrationRequest(settingsJson));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IntegrationConnectionResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<IntegrationSyncRunResponse> SyncAsync(HttpClient client, string provider)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, $"/api/integrations/{provider}/sync");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IntegrationSyncRunResponse>(
            TestContext.Current.CancellationToken))!;
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
