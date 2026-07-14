using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Contracts.Auth;
using NorvixHub.Contracts.LiveDemo;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.LiveDemo;

public sealed class LiveDemoRunEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public LiveDemoRunEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_run_queues_preset_tenant_scoped_steps()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);

        using var response = await SendCreateAsync(client, session.Token, new CreateLiveDemoRunRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var created = await response.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken);
        created.Should().NotBeNull();
        created!.Status.Should().Be("Queued");
        created.PollUrl.Should().Be($"/api/live-demo-runs/{created.RunId}");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var run = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == created.RunId,
            TestContext.Current.CancellationToken);
        run.TenantId.Should().Be(session.DemoTenantId);
        run.Status.Should().Be(LiveDemoRunStatus.Queued);
        run.OrganizationNumber.Should().Be("999888777");
        run.CustomerReference.Should().StartWith("LIVE-");
        var steps = await dbContext.LiveDemoRunSteps
            .Where(candidate => candidate.RunId == run.Id)
            .OrderBy(candidate => candidate.Sequence)
            .ToListAsync(TestContext.Current.CancellationToken);
        steps.Select(step => step.Key).Should().Equal(
            "request-created", "brreg-checked", "case-created", "document-created",
            "sharepoint-synced", "erp-received", "run-completed");
        steps.Where(step => step.Key != "erp-received")
            .Should().OnlyContain(step => step.Status == LiveDemoRunStepStatus.Pending);
        steps.Single(step => step.Key == "erp-received").Status
            .Should().Be(LiveDemoRunStepStatus.Skipped);
    }

    [Fact]
    public async Task Create_run_rejects_unauthenticated_request()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/live-demo-runs",
            new CreateLiveDemoRunRequest(),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_run_isolated_between_demo_tenants()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var first = await CreateDemoSessionAsync(client);
        var second = await CreateDemoSessionAsync(client);

        using var firstResponse = await SendCreateAsync(client, first.Token, new CreateLiveDemoRunRequest());
        using var secondResponse = await SendCreateAsync(client, second.Token, new CreateLiveDemoRunRequest());
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var firstRun = (await firstResponse.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;
        var secondRun = (await secondResponse.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        (await dbContext.LiveDemoRuns.SingleAsync(
            run => run.Id == firstRun.RunId,
            TestContext.Current.CancellationToken)).TenantId
            .Should().Be(first.DemoTenantId);
        (await dbContext.LiveDemoRuns.SingleAsync(
            run => run.Id == secondRun.RunId,
            TestContext.Current.CancellationToken)).TenantId
            .Should().Be(second.DemoTenantId);
    }

    [Fact]
    public async Task Create_run_rejects_a_second_active_run_for_same_session()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);

        using var first = await SendCreateAsync(client, session.Token, new CreateLiveDemoRunRequest());
        using var second = await SendCreateAsync(client, session.Token, new CreateLiveDemoRunRequest());

        first.StatusCode.Should().Be(HttpStatusCode.Accepted);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_run_ignores_arbitrary_scenario_fields_from_browser()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/live-demo-runs")
        {
            Content = JsonContent.Create(new
            {
                simulateErpFailureOnce = true,
                organizationNumber = "000000000",
                customerReference = "ATTACKER-REFERENCE",
                requestTitle = "Attacker title",
                requestBody = "Attacker body"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var created = (await response.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var run = await dbContext.LiveDemoRuns.SingleAsync(
            run => run.Id == created.RunId,
            TestContext.Current.CancellationToken);
        run.SimulateErpFailureOnce.Should().BeTrue();
        run.OrganizationNumber.Should().Be("999888777");
        run.CustomerReference.Should().NotBe("ATTACKER-REFERENCE");
        run.RequestTitle.Should().NotBe("Attacker title");
        run.RequestBody.Should().NotBe("Attacker body");
    }

    [Fact]
    public async Task Create_run_returns_safe_error_when_feature_is_disabled()
    {
        using var factory = CreateFactory(enabled: false);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);

        using var response = await SendCreateAsync(client, session.Token, new CreateLiveDemoRunRequest());

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("Live demo is not available.");
        body.Should().NotContain("OrganizationNumber");
    }

    [Fact]
    public async Task Create_run_does_not_execute_or_create_workflow_artifacts()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var casesBefore = await dbContext.Cases.CountAsync(
            caseItem => caseItem.TenantId == session.DemoTenantId,
            TestContext.Current.CancellationToken);
        var documentsBefore = await dbContext.Documents.CountAsync(
            document => document.TenantId == session.DemoTenantId,
            TestContext.Current.CancellationToken);

        using var response = await SendCreateAsync(client, session.Token, new CreateLiveDemoRunRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await dbContext.Cases.CountAsync(
            caseItem => caseItem.TenantId == session.DemoTenantId,
            TestContext.Current.CancellationToken)).Should().Be(casesBefore);
        (await dbContext.Documents.CountAsync(
            document => document.TenantId == session.DemoTenantId,
            TestContext.Current.CancellationToken)).Should().Be(documentsBefore);
    }

    [Fact]
    public async Task Get_run_returns_own_queued_run_with_stably_ordered_steps()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        using var createResponse = await SendCreateAsync(client, session.Token, new CreateLiveDemoRunRequest());
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;

        using var response = await SendGetAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var run = (await response.Content.ReadFromJsonAsync<LiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;
        run.RunId.Should().Be(created.RunId);
        run.Status.Should().Be("Queued");
        run.Result.Should().BeNull();
        run.CanRetry.Should().BeFalse();
        run.Steps.Select(step => step.Sequence).Should().BeInAscendingOrder();
        run.Steps.Select(step => step.Key).Should().Equal(
            "request-created", "brreg-checked", "case-created", "document-created",
            "sharepoint-synced", "erp-received", "run-completed");
    }

    [Fact]
    public async Task Get_run_returns_not_found_for_another_demo_tenant()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var owner = await CreateDemoSessionAsync(client);
        var other = await CreateDemoSessionAsync(client);
        using var createResponse = await SendCreateAsync(client, owner.Token, new CreateLiveDemoRunRequest());
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;

        using var response = await SendGetAsync(client, other.Token, $"/api/live-demo-runs/{created.RunId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_completed_run_shortens_external_references_and_hides_configuration()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        using var createResponse = await SendCreateAsync(client, session.Token, new CreateLiveDemoRunRequest());
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;
        const string fullFolderId = "folder-reference-that-must-not-be-exposed-in-full";
        const string fullFileId = "file-reference-that-must-not-be-exposed-in-full";
        const string fullReceiptId = "erp-receipt-that-must-not-be-exposed-in-full";
        await CompleteRunAsync(factory, created.RunId, fullFolderId, fullFileId, fullReceiptId);

        using var response = await SendGetAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var run = await response.Content.ReadFromJsonAsync<LiveDemoRunResponse>(
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        run!.Result.Should().NotBeNull();
        run.Result!.SharePointFolderReference.Should().Be("folder-r…n-full");
        run.Result.SharePointFileReference.Should().Be("file-ref…n-full");
        run.Result.ErpReceiptId.Should().Be(fullReceiptId);
        body.Should().NotContain(fullFolderId);
        body.Should().NotContain(fullFileId);
        body.Should().NotContain("999888777");
        body.Should().NotContain("correlationId");
    }

    [Fact]
    public async Task Capabilities_map_only_safe_boolean_values()
    {
        using var enabledFactory = CreateFactory(enabled: true, erpEnabled: true);
        using var enabledClient = enabledFactory.CreateClient();
        var enabledSession = await CreateDemoSessionAsync(enabledClient);
        using var enabledResponse = await SendGetAsync(
            enabledClient,
            enabledSession.Token,
            "/api/live-demo-capabilities");
        var enabled = (await enabledResponse.Content.ReadFromJsonAsync<LiveDemoCapabilitiesResponse>(
            TestContext.Current.CancellationToken))!;

        enabledResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        enabled.Enabled.Should().BeTrue();
        enabled.BrregLiveEnabled.Should().BeTrue();
        enabled.SharePointSimulatorEnabled.Should().BeTrue();
        enabled.ErpReceiverEnabled.Should().BeFalse();
        enabled.FailureDemoEnabled.Should().BeFalse();

        using var disabledFactory = CreateFactory(enabled: false);
        using var disabledClient = disabledFactory.CreateClient();
        var disabledSession = await CreateDemoSessionAsync(disabledClient);
        using var disabledResponse = await SendGetAsync(
            disabledClient,
            disabledSession.Token,
            "/api/live-demo-capabilities");
        var disabled = (await disabledResponse.Content.ReadFromJsonAsync<LiveDemoCapabilitiesResponse>(
            TestContext.Current.CancellationToken))!;

        disabled.Enabled.Should().BeFalse();
        disabled.BrregLiveEnabled.Should().BeFalse();
        disabled.SharePointSimulatorEnabled.Should().BeFalse();
        disabled.ErpReceiverEnabled.Should().BeFalse();
        disabled.FailureDemoEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Retry_failed_run_queues_retry_and_writes_audit_event()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);
        await FailRunAsync(factory, created.RunId, markSteps: true);

        using var response = await SendPostAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}/retry");

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var retry = (await response.Content.ReadFromJsonAsync<RetryLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;
        retry.Status.Should().Be("Queued");
        retry.RetryCount.Should().Be(1);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var run = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == created.RunId,
            TestContext.Current.CancellationToken);
        run.Status.Should().Be(LiveDemoRunStatus.Queued);
        run.CaseId.Should().NotBeNull();
        (await dbContext.AuditEvents.AnyAsync(
            candidate => candidate.TenantId == session.DemoTenantId &&
                candidate.EntityId == created.RunId.ToString() &&
                candidate.Action == "LiveDemoRunRetried",
            TestContext.Current.CancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task Retry_rejects_completed_run()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);
        await CompleteRunAsync(factory, created.RunId, "folder", "file", "receipt");

        using var response = await SendPostAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}/retry");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Retry_returns_not_found_for_another_tenant()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var owner = await CreateDemoSessionAsync(client);
        var other = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, owner.Token);
        await FailRunAsync(factory, created.RunId, markSteps: true);

        using var response = await SendPostAsync(client, other.Token, $"/api/live-demo-runs/{created.RunId}/retry");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Retry_enforces_configured_retry_limit()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);
        await FailRunAsync(factory, created.RunId, markSteps: true);

        using var firstRetry = await SendPostAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}/retry");
        firstRetry.StatusCode.Should().Be(HttpStatusCode.Accepted);
        await FailRunAsync(factory, created.RunId, markSteps: false);
        using var secondRetry = await SendPostAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}/retry");
        secondRetry.StatusCode.Should().Be(HttpStatusCode.Accepted);
        await FailRunAsync(factory, created.RunId, markSteps: false);

        using var response = await SendPostAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}/retry");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Retry_preserves_completed_steps_and_artifact_ids()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var created = await CreateRunAsync(client, session.Token);
        await FailRunAsync(factory, created.RunId, markSteps: true);

        using var response = await SendPostAsync(client, session.Token, $"/api/live-demo-runs/{created.RunId}/retry");
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var steps = await dbContext.LiveDemoRunSteps
            .Where(candidate => candidate.RunId == created.RunId)
            .OrderBy(candidate => candidate.Sequence)
            .ToListAsync(TestContext.Current.CancellationToken);
        steps[0].Status.Should().Be(LiveDemoRunStepStatus.Completed);
        steps[0].AttemptCount.Should().Be(1);
        steps[1].Status.Should().Be(LiveDemoRunStepStatus.Pending);
        steps[1].PublicErrorCode.Should().BeNull();
        (await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == created.RunId,
            TestContext.Current.CancellationToken)).CaseId.Should().NotBeNull();
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateFactory(
        bool enabled,
        bool erpEnabled = false)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = enabled.ToString(),
                    ["LiveDemo:OrganizationNumber"] = "999888777",
                    ["LiveDemo:MaxRunsPerSession"] = "3",
                    ["ErpDemo:Enabled"] = erpEnabled.ToString(),
                    ["SharePoint:Mode"] = "Simulated"
                });
            });
        });
    }

    private static async Task<CreateDemoSessionResponse> CreateDemoSessionAsync(HttpClient client)
    {
        using var response = await client.PostAsync("/api/demo-sessions", null, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateDemoSessionResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static Task<HttpResponseMessage> SendCreateAsync(
        HttpClient client,
        string token,
        CreateLiveDemoRunRequest request)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/live-demo-runs")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client.SendAsync(message, TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> SendGetAsync(HttpClient client, string token, string path)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, path);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(message, TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> SendPostAsync(HttpClient client, string token, string path)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, path);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(message, TestContext.Current.CancellationToken);
    }

    private static async Task<CreateLiveDemoRunResponse> CreateRunAsync(HttpClient client, string token)
    {
        using var response = await SendCreateAsync(client, token, new CreateLiveDemoRunRequest());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateLiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task FailRunAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        Guid runId,
        bool markSteps)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var run = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == runId,
            TestContext.Current.CancellationToken);
        var now = DateTimeOffset.UtcNow;
        run.MarkRunning("erp-received", now);
        if (markSteps)
        {
            var steps = await dbContext.LiveDemoRunSteps
                .Where(candidate => candidate.RunId == runId)
                .OrderBy(candidate => candidate.Sequence)
                .ToListAsync(TestContext.Current.CancellationToken);
            steps[0].MarkRunning(now);
            steps[0].MarkCompleted("Henvendelse registrert.", "RUN-0142", now);
            steps[1].MarkRunning(now);
            steps[1].MarkFailed("ERP_RECEIVER_UNAVAILABLE", "ERP demo receiver svarer ikke.", now);
            run.SetInternalArtifacts(null, null, Guid.NewGuid(), null, null, now);
        }

        run.MarkFailed("ERP_RECEIVER_UNAVAILABLE", "ERP demo receiver svarer ikke.", now);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static async Task CompleteRunAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        Guid runId,
        string folderId,
        string fileId,
        string receiptId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var run = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == runId,
            TestContext.Current.CancellationToken);
        var now = DateTimeOffset.UtcNow;
        run.MarkRunning("sharepoint-synced", now);
        run.SetBrregEvidence("live", now, now);
        run.SetSharePointEvidence("drive", folderId, fileId, now);
        run.SetErpReceipt(receiptId, now);
        run.MarkCompleted(now.AddSeconds(1));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
