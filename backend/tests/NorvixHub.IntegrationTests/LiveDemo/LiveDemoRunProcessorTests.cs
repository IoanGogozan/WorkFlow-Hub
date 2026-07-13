using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NorvixHub.Application.Documents;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Application.Organizations;
using NorvixHub.Contracts.Auth;
using NorvixHub.Contracts.LiveDemo;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.LiveDemo;

public sealed class LiveDemoRunProcessorTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public LiveDemoRunProcessorTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Processor_completes_internal_brreg_and_simulated_sharepoint_steps()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777"
                });
            });
        });
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, session.Token);

        using (var processingScope = factory.Services.CreateScope())
        {
            var processor = processingScope.ServiceProvider.GetRequiredService<ILiveDemoRunProcessor>();
            await processor.ProcessAsync(run.RunId, TestContext.Current.CancellationToken);
        }

        using var verificationScope = factory.Services.CreateScope();
        var dbContext = verificationScope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var persistedRun = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == run.RunId,
            TestContext.Current.CancellationToken);
        var steps = await dbContext.LiveDemoRunSteps
            .Where(candidate => candidate.RunId == run.RunId)
            .OrderBy(candidate => candidate.Sequence)
            .ToListAsync(TestContext.Current.CancellationToken);

        persistedRun.Status.Should().Be(LiveDemoRunStatus.Completed);
        persistedRun.TotalDurationMs.Should().NotBeNull();
        persistedRun.IntakeItemId.Should().NotBeNull();
        persistedRun.CustomerId.Should().NotBeNull();
        persistedRun.CaseId.Should().NotBeNull();
        persistedRun.DocumentId.Should().NotBeNull();
        persistedRun.DeliveryPackageId.Should().NotBeNull();
        persistedRun.BrregMode.Should().Be("live");
        steps.Where(step => step.Key is "request-created" or "brreg-checked" or "case-created" or "document-created" or "sharepoint-synced" or "run-completed")
            .Should().OnlyContain(step => step.Status == LiveDemoRunStepStatus.Completed);
        steps.Where(step => step.Key is "erp-received")
            .Should().OnlyContain(step => step.Status == LiveDemoRunStepStatus.Pending);
        (await dbContext.AuditEvents.CountAsync(
            candidate => candidate.TenantId == session.DemoTenantId &&
                candidate.EntityId == run.RunId.ToString() &&
                candidate.Action == "LiveDemoStepCompleted",
            TestContext.Current.CancellationToken)).Should().Be(6);
        var sharePointStep = steps.Single(step => step.Key == "sharepoint-synced");
        sharePointStep.EvidenceMode.Should().Be("simulated-sharepoint");
        sharePointStep.PublicSummary.Should().Contain("no Microsoft 365 tenant connected");
        persistedRun.SharePointDriveId.Should().NotBeNull();
        persistedRun.SharePointFolderItemId.Should().NotBeNull().And.NotContain("/");
        persistedRun.SharePointFileItemId.Should().NotBeNull();
        persistedRun.SharePointDriveId!.Length.Should().BeLessThanOrEqualTo(16);
        persistedRun.SharePointFolderItemId!.Length.Should().BeLessThanOrEqualTo(16);
        persistedRun.SharePointFileItemId!.Length.Should().BeLessThanOrEqualTo(16);

        var customer = await dbContext.Customers.SingleAsync(
            candidate => candidate.Id == persistedRun.CustomerId,
            TestContext.Current.CancellationToken);
        customer.Name.Should().Be("Sordal Eiendom AS");
        customer.OrganizationNumber.Should().Be("999888777");
        customer.BrregDataJson.Should().Contain("Sordal Eiendom AS");

        var version = await dbContext.DocumentVersions.SingleAsync(
            candidate => candidate.DocumentId == persistedRun.DocumentId,
            TestContext.Current.CancellationToken);
        var storage = verificationScope.ServiceProvider.GetRequiredService<IFileStorage>();
        var content = await storage.OpenReadAsync(
            version.BlobContainer,
            version.BlobName,
            TestContext.Current.CancellationToken);
        content.Should().NotBeNull();
        await using (content!.Content)
        {
            using var bytes = new MemoryStream();
            await content.Content.CopyToAsync(bytes, TestContext.Current.CancellationToken);
            bytes.ToArray().Should().StartWith("%PDF"u8.ToArray());
        }
    }

    [Fact]
    public async Task Processor_repeated_calls_and_new_scopes_do_not_duplicate_artifacts()
    {
        using var factory = CreateLiveDemoFactory();
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, session.Token);

        await ProcessAsync(factory, run.RunId);
        var before = await GetArtifactCountsAsync(factory, run.RunId, session.DemoTenantId);
        await ProcessAsync(factory, run.RunId);
        var after = await GetArtifactCountsAsync(factory, run.RunId, session.DemoTenantId);

        after.Should().Be(before);
    }

    [Fact]
    public async Task Processor_marks_brreg_fallback_clearly_without_exposing_internal_data()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777",
                    ["LiveDemo:BrregFallbackEnabled"] = "true"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBrregClient>();
                services.AddScoped<IBrregClient, UnavailableBrregClient>();
            });
        });
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, session.Token);

        await ProcessAsync(factory, run.RunId);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var persistedRun = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == run.RunId,
            TestContext.Current.CancellationToken);
        persistedRun.BrregMode.Should().Be("fallback");
        var brregStep = await dbContext.LiveDemoRunSteps.SingleAsync(
            candidate => candidate.RunId == run.RunId && candidate.Key == "brreg-checked",
            TestContext.Current.CancellationToken);
        brregStep.PublicSummary.Should().Contain("fallback-snapshot");
        brregStep.PublicSummary.Should().NotContain("upstream-secret");
        var customer = await dbContext.Customers.SingleAsync(
            candidate => candidate.Id == persistedRun.CustomerId,
            TestContext.Current.CancellationToken);
        customer.Name.Should().Be("Fiktiv Brreg demo snapshot AS");
    }

    [Fact]
    public async Task Processor_keeps_live_demo_artifacts_tenant_scoped()
    {
        using var factory = CreateLiveDemoFactory();
        using var client = factory.CreateClient();
        var firstSession = await CreateDemoSessionAsync(client);
        var secondSession = await CreateDemoSessionAsync(client);
        var firstRun = await CreateRunAsync(client, firstSession.Token);
        var secondRun = await CreateRunAsync(client, secondSession.Token);

        await ProcessAsync(factory, firstRun.RunId);
        await ProcessAsync(factory, secondRun.RunId);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var first = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == firstRun.RunId,
            TestContext.Current.CancellationToken);
        var second = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == secondRun.RunId,
            TestContext.Current.CancellationToken);
        first.TenantId.Should().Be(firstSession.DemoTenantId);
        second.TenantId.Should().Be(secondSession.DemoTenantId);
        (await dbContext.Documents.SingleAsync(
            candidate => candidate.Id == first.DocumentId,
            TestContext.Current.CancellationToken)).TenantId.Should().Be(firstSession.DemoTenantId);
        (await dbContext.Documents.SingleAsync(
            candidate => candidate.Id == second.DocumentId,
            TestContext.Current.CancellationToken)).TenantId.Should().Be(secondSession.DemoTenantId);
    }

    [Fact]
    public async Task Processor_sends_fictional_ERP_payload_and_persists_receipt_evidence()
    {
        var erpClient = new FakeErpDemoClient(new ErpDemoResult(
            ErpDemoResultStatus.Received,
            "ERP-DEMO-NORMAL001",
            false,
            DateTime.UtcNow));
        using var factory = CreateErpDemoFactory(erpClient);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, session.Token);

        await ProcessAsync(factory, run.RunId);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var persistedRun = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == run.RunId,
            TestContext.Current.CancellationToken);
        var erpStep = await dbContext.LiveDemoRunSteps.SingleAsync(
            candidate => candidate.RunId == run.RunId && candidate.Key == "erp-received",
            TestContext.Current.CancellationToken);
        persistedRun.Status.Should().Be(LiveDemoRunStatus.Completed);
        persistedRun.ErpReceiptId.Should().Be("ERP-DEMO-NORMAL001");
        erpStep.Status.Should().Be(LiveDemoRunStepStatus.Completed);
        erpStep.AttemptCount.Should().Be(1);
        erpStep.DurationMs.Should().NotBeNull();
        erpClient.Requests.Should().ContainSingle();
        var request = erpClient.Requests.Single();
        request.RunId.Should().Be(run.RunId);
        request.CustomerReference.Should().StartWith("FICTIONAL-LIVE-");
        request.CaseNumber.Should().StartWith("LIVE-");
        request.DocumentReference.Should().EndWith(".pdf");
        request.FailOnce.Should().BeFalse();

        var auditPayloads = await dbContext.AuditEvents
            .Where(candidate => candidate.EntityId == run.RunId.ToString())
            .Select(candidate => (candidate.BeforeJson ?? string.Empty) + (candidate.AfterJson ?? string.Empty))
            .ToListAsync(TestContext.Current.CancellationToken);
        auditPayloads.Should().OnlyContain(payload => !payload.Contains("signature", StringComparison.OrdinalIgnoreCase));
        auditPayloads.Should().OnlyContain(payload => !payload.Contains("X-Norvix", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ERP_failure_and_retry_resume_without_duplicate_artifacts_or_client_calls()
    {
        var erpClient = new FakeErpDemoClient(
            new ErpDemoResult(ErpDemoResultStatus.Unavailable),
            new ErpDemoResult(ErpDemoResultStatus.Received, "ERP-DEMO-RETRY001", false, DateTime.UtcNow));
        using var factory = CreateErpDemoFactory(erpClient);
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, session.Token, simulateErpFailureOnce: true);

        await ProcessAsync(factory, run.RunId);
        var afterFailure = await GetArtifactCountsAsync(factory, run.RunId, session.DemoTenantId);
        using (var failedScope = factory.Services.CreateScope())
        {
            var failedDb = failedScope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
            (await failedDb.LiveDemoRuns.SingleAsync(
                candidate => candidate.Id == run.RunId,
                TestContext.Current.CancellationToken)).Status.Should().Be(LiveDemoRunStatus.Failed);
        }

        await RetryRunAsync(client, run.RunId, session.Token);
        await ProcessAsync(factory, run.RunId);
        await ProcessAsync(factory, run.RunId);
        var afterRetry = await GetArtifactCountsAsync(factory, run.RunId, session.DemoTenantId);

        afterRetry.Should().Be(afterFailure);
        erpClient.Requests.Should().HaveCount(2);
        erpClient.Requests.Should().OnlyContain(request => request.RunId == run.RunId && request.FailOnce);
        using var completedScope = factory.Services.CreateScope();
        var completedDb = completedScope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var completedRun = await completedDb.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == run.RunId,
            TestContext.Current.CancellationToken);
        var erpStep = await completedDb.LiveDemoRunSteps.SingleAsync(
            candidate => candidate.RunId == run.RunId && candidate.Key == "erp-received",
            TestContext.Current.CancellationToken);
        completedRun.Status.Should().Be(LiveDemoRunStatus.Completed);
        completedRun.ErpReceiptId.Should().Be("ERP-DEMO-RETRY001");
        completedRun.RetryCount.Should().Be(1);
        erpStep.Status.Should().Be(LiveDemoRunStepStatus.Completed);
        erpStep.AttemptCount.Should().Be(2);
    }

    [Fact]
    public async Task Another_tenant_cannot_read_run_with_ERP_receipt()
    {
        var erpClient = new FakeErpDemoClient(new ErpDemoResult(
            ErpDemoResultStatus.Received,
            "ERP-DEMO-TENANT01",
            false,
            DateTime.UtcNow));
        using var factory = CreateErpDemoFactory(erpClient);
        using var client = factory.CreateClient();
        var ownerSession = await CreateDemoSessionAsync(client);
        var otherSession = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, ownerSession.Token);
        await ProcessAsync(factory, run.RunId);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/live-demo-runs/{run.RunId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherSession.Token);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateLiveDemoFactory()
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777"
                });
            });
        });
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateErpDemoFactory(
        FakeErpDemoClient erpClient)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777",
                    ["ErpDemo:Enabled"] = "true"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IErpDemoClient>();
                services.AddSingleton<IErpDemoClient>(erpClient);
            });
        });
    }

    private static async Task ProcessAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        Guid runId)
    {
        using var scope = factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<ILiveDemoRunProcessor>();
        await processor.ProcessAsync(runId, TestContext.Current.CancellationToken);
    }

    private static async Task<ArtifactCounts> GetArtifactCountsAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        Guid runId,
        Guid tenantId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var run = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == runId,
            TestContext.Current.CancellationToken);
        return new ArtifactCounts(
            await dbContext.IntakeItems.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            await dbContext.Customers.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            await dbContext.Cases.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            await dbContext.Documents.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            await dbContext.DocumentVersions.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            await dbContext.DeliveryPackages.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            await dbContext.SimulatedSharePointDocumentItems.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            await dbContext.SimulatedSharePointOperations.CountAsync(candidate => candidate.TenantId == tenantId, TestContext.Current.CancellationToken),
            run.IntakeItemId,
            run.CustomerId,
            run.CaseId,
            run.DocumentId,
            run.DeliveryPackageId);
    }

    private sealed record ArtifactCounts(
        int Intakes,
        int Customers,
        int Cases,
        int Documents,
        int Versions,
        int DeliveryPackages,
        int SharePointItems,
        int SharePointOperations,
        Guid? IntakeId,
        Guid? CustomerId,
        Guid? CaseId,
        Guid? DocumentId,
        Guid? DeliveryPackageId);

    private sealed class UnavailableBrregClient : IBrregClient
    {
        public Task<IReadOnlyList<BrregOrganization>> SearchAsync(string query, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BrregOrganization>>([]);

        public Task<BrregOrganization?> GetByOrganizationNumberAsync(
            string organizationNumber,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("upstream-secret");
    }

    private static async Task<CreateDemoSessionResponse> CreateDemoSessionAsync(HttpClient client)
    {
        using var response = await client.PostAsync("/api/demo-sessions", null, TestContext.Current.CancellationToken);
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

    private static async Task RetryRunAsync(HttpClient client, Guid runId, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/live-demo-runs/{runId}/retry");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    private sealed class FakeErpDemoClient(params ErpDemoResult[] results) : IErpDemoClient
    {
        private readonly Queue<ErpDemoResult> results = new(results);

        public List<ErpDemoRequest> Requests { get; } = [];

        public Task<ErpDemoResult> SendAsync(ErpDemoRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(results.Count > 0
                ? results.Dequeue()
                : new ErpDemoResult(ErpDemoResultStatus.InvalidResponse));
        }
    }
}
