using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Contracts.Auth;
using NorvixHub.Contracts.LiveDemo;
using NorvixHub.Contracts.LiveDemoEvidence;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.LiveDemo;

public sealed class LiveDemoEvidenceEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public LiveDemoEvidenceEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LiveDemoEvidence_returns_exact_run_artifacts_operations_and_ordered_audit()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);
        await ProcessAsync(factory, created.RunId);

        using var response = await GetEvidenceAsync(client, session.Token, created.RunId);
        var evidence = await response.Content.ReadFromJsonAsync<LiveDemoEvidenceResponse>(
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        evidence.Should().NotBeNull();
        evidence!.Run.RunId.Should().Be(created.RunId);
        evidence.Request!.SourceLabel.Should().Be("Fiktiv henvendelse");
        evidence.Brreg!.Mode.Should().Be("live");
        evidence.Case.Should().NotBeNull();
        evidence.Document.Should().NotBeNull();
        evidence.SharePoint.Should().NotBeNull();
        evidence.SharePoint!.Mode.Should().Be("simulated");
        evidence.SharePoint.Operations.Should().NotBeEmpty();
        evidence.AuditEvents.Select(item => item.Timestamp).Should().BeInAscendingOrder();
        evidence.AuditEvents.Should().Contain(item =>
            item.Provider == "Brreg" && item.DurationMs.HasValue && item.Attempt == 1);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var run = await db.LiveDemoRuns.AsNoTracking().SingleAsync(
            item => item.Id == created.RunId,
            TestContext.Current.CancellationToken);
        var exactCase = await db.Cases.AsNoTracking().SingleAsync(
            item => item.Id == run.CaseId,
            TestContext.Current.CancellationToken);
        evidence.Case!.CaseNumber.Should().Be(exactCase.CaseNumber);
        evidence.Case.CaseHref.Should().Be($"/cases/{run.CaseId}");
        evidence.Document!.DocumentId.Should().Be(run.DocumentId!.Value);
        evidence.Document.DocumentHref.Should().Be($"/documents/{run.DocumentId}");
    }

    [Fact]
    public async Task LiveDemoEvidence_returns_not_found_for_another_tenant_and_unauthorized_without_authentication()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var owner = await CreateDemoSessionAsync(client);
        var other = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, owner.Token);

        using var crossTenant = await GetEvidenceAsync(client, other.Token, created.RunId);
        using var unauthenticated = await client.GetAsync(
            $"/api/live-demo-runs/{created.RunId}/evidence",
            TestContext.Current.CancellationToken);

        crossTenant.StatusCode.Should().Be(HttpStatusCode.NotFound);
        unauthenticated.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Completed_run_result_links_to_its_evidence_and_exact_artifacts()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);
        await ProcessAsync(factory, created.RunId);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/live-demo-runs/{created.RunId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var run = await response.Content.ReadFromJsonAsync<LiveDemoRunResponse>(
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        run!.Result.Should().NotBeNull();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var persisted = await db.LiveDemoRuns.AsNoTracking().SingleAsync(
            item => item.Id == created.RunId,
            TestContext.Current.CancellationToken);
        run.Result!.EvidenceHref.Should().Be($"/technical/live-runs/{created.RunId}");
        run.Result.CaseHref.Should().Be($"/cases/{persisted.CaseId}");
        run.Result.DocumentHref.Should().Be($"/documents/{persisted.DocumentId}");
        run.Result.DocumentFileName.Should().EndWith(".pdf");
        run.Result.DocumentDownloadHref.Should().Be($"/api/documents/{persisted.DocumentId}/download");
        run.Result.SharePointEvidenceHref.Should().Be($"/technical/live-runs/{created.RunId}#sharepoint");
        run.Result.AuditHref.Should().Be($"/technical/live-runs/{created.RunId}#audit");
    }

    [Fact]
    public async Task LiveDemoEvidence_represents_missing_artifacts_without_server_error()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);

        using var response = await GetEvidenceAsync(client, session.Token, created.RunId);
        var evidence = await response.Content.ReadFromJsonAsync<LiveDemoEvidenceResponse>(
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        evidence!.Run.Status.Should().Be("Queued");
        evidence.Request.Should().NotBeNull();
        evidence.Brreg.Should().BeNull();
        evidence.Case.Should().BeNull();
        evidence.Document.Should().BeNull();
        evidence.SharePoint.Should().BeNull();
        evidence.Erp.Should().BeNull();
    }

    [Fact]
    public async Task LiveDemoEvidence_response_omits_secrets_paths_network_data_and_raw_payloads()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);
        await ProcessAsync(factory, created.RunId);

        using var response = await GetEvidenceAsync(client, session.Token, created.RunId);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotContain("connectionString");
        body.Should().NotContain("blobContainer");
        body.Should().NotContain("blobName");
        body.Should().NotContain("accessToken");
        body.Should().NotContain("settingsJson");
        body.Should().NotContain("userAgent");
        body.Should().NotContain("ipAddress");
        body.Should().NotContain("hmac");
        body.Should().NotContain("requestSummaryJson");
        body.Should().NotContain("responseSummaryJson");
    }

    [Fact]
    public async Task LiveDemoEvidence_returns_self_hosted_ERP_receipt_attempts_and_retry_history()
    {
        var erpClient = new EvidenceErpDemoClient();
        using var factory = CreateErpFactory(erpClient);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token, simulateErpFailureOnce: true);
        await ProcessAsync(factory, created.RunId);
        await RetryRunAsync(client, created.RunId, session.Token);
        await ProcessAsync(factory, created.RunId);

        using var response = await GetEvidenceAsync(client, session.Token, created.RunId);
        var evidence = await response.Content.ReadFromJsonAsync<LiveDemoEvidenceResponse>(
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        evidence!.Erp.Should().NotBeNull();
        evidence.Erp!.Mode.Should().Be("self-hosted");
        evidence.Erp.Status.Should().Be("Received");
        evidence.Erp.ExternalReceiptId.Should().Be("ERP-DEMO-EVIDENCE01");
        evidence.Erp.IdempotencyKey.Should().StartWith("live-dem").And.Contain("…");
        evidence.Erp.Attempts.Should().Be(2);
        evidence.Erp.LastDurationMs.Should().NotBeNull();
        evidence.Erp.History.Select(item => item.Status).Should().Equal("Failed", "Received");
        evidence.Erp.History.Select(item => item.Attempt).Should().Equal(1, 2);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().NotContain("X-Norvix-Signature");
        body.Should().NotContain("SigningSecret");
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateFactory() =>
        _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(configuration =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777"
                }));
        });

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateErpFactory(
        EvidenceErpDemoClient erpClient) =>
        _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(configuration =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777",
                    ["ErpDemo:Enabled"] = "true"
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IErpDemoClient>();
                services.AddSingleton<IErpDemoClient>(erpClient);
            });
        });

    private static async Task<CreateDemoSessionResponse> CreateDemoSessionAsync(HttpClient client)
    {
        using var response = await client.PostAsync(
            "/api/demo-sessions", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreateDemoSessionResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<CreateLiveDemoRunResponse> CreateRunAsync(HttpClient client, string token)
        => await CreateRunAsync(client, token, simulateErpFailureOnce: false);

    private static async Task<CreateLiveDemoRunResponse> CreateRunAsync(
        HttpClient client,
        string token,
        bool simulateErpFailureOnce)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/live-demo-runs")
        {
            Content = JsonContent.Create(new CreateLiveDemoRunRequest(simulateErpFailureOnce))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        return (await response.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task ProcessAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        Guid runId)
    {
        using var scope = factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<ILiveDemoRunProcessor>();
        await processor.ProcessAsync(runId, TestContext.Current.CancellationToken);
    }

    private static Task<HttpResponseMessage> GetEvidenceAsync(
        HttpClient client,
        string token,
        Guid runId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get, $"/api/live-demo-runs/{runId}/evidence");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task RetryRunAsync(HttpClient client, Guid runId, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/live-demo-runs/{runId}/retry");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    private sealed class EvidenceErpDemoClient : IErpDemoClient
    {
        private int attempts;

        public Task<ErpDemoResult> SendAsync(ErpDemoRequest request, CancellationToken cancellationToken)
        {
            attempts++;
            return Task.FromResult(attempts == 1
                ? new ErpDemoResult(ErpDemoResultStatus.Unavailable)
                : new ErpDemoResult(
                    ErpDemoResultStatus.Received,
                    "ERP-DEMO-EVIDENCE01",
                    false,
                    DateTime.UtcNow));
        }
    }
}
