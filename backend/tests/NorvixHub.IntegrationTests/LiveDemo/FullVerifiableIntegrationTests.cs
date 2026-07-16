extern alias erpreceiver;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Contracts.Auth;
using NorvixHub.Contracts.Cases;
using NorvixHub.Contracts.LiveDemo;
using NorvixHub.Contracts.LiveDemoEvidence;
using NorvixHub.Infrastructure.LiveDemo;
using NorvixHub.IntegrationTests.Support;
using ErpReceiverDbContext = erpreceiver::NorvixHub.ErpDemoReceiver.Persistence.ErpDemoReceiverDbContext;
using ErpReceiverProgram = erpreceiver::Program;
using Xunit;

namespace NorvixHub.IntegrationTests.LiveDemo;

public sealed class FullVerifiableIntegrationTests : IClassFixture<NorvixHubApiFactory>
{
    private const string SigningSecret = "full-e2e-signing-secret-with-test-only-entropy";
    private readonly NorvixHubApiFactory apiFactory;

    public FullVerifiableIntegrationTests(NorvixHubApiFactory apiFactory)
    {
        this.apiFactory = apiFactory;
    }

    [Fact]
    public async Task Normal_run_reaches_real_receiver_and_exposes_matching_openable_evidence()
    {
        var receiverDatabase = Path.Combine(Path.GetTempPath(), $"norvixhub-erp-e2e-{Guid.NewGuid():N}.db");
        try
        {
            using var receiverFactory = new WebApplicationFactory<ErpReceiverProgram>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Demo");
                    builder.ConfigureAppConfiguration(configuration =>
                        configuration.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:ErpDemoReceiver"] = $"Data Source={receiverDatabase}",
                            ["ErpDemoReceiver:SigningSecret"] = SigningSecret,
                            ["ErpDemoReceiver:EnableFailOnce"] = "true"
                        }));
                });
            using var receiverClient = receiverFactory.CreateClient();
            using var factory = CreateApplicationFactory(receiverClient);
            using var client = factory.CreateClient();

            var session = await CreateDemoSessionAsync(client);
            var created = await CreateRunAsync(client, session.Token);
            await ProcessAsync(factory, created.RunId);

            var run = await GetRunAsync(client, session.Token, created.RunId);
            run.Status.Should().Be("Completed");
            run.Result.Should().NotBeNull();
            run.Result!.EvidenceHref.Should().Be($"/technical/live-runs/{created.RunId}");
            run.Result.ErpReceiptId.Should().StartWith("ERP-DEMO-");

            var evidence = await GetEvidenceAsync(client, session.Token, created.RunId);
            evidence.Run.RunId.Should().Be(created.RunId);
            evidence.Run.Status.Should().Be("Completed");
            evidence.Brreg!.Mode.Should().Be("live");
            evidence.Brreg.OrganizationNumber.Should().Be("999888777");
            evidence.Case.Should().NotBeNull();
            evidence.Document.Should().NotBeNull();
            evidence.SharePoint.Should().NotBeNull();
            evidence.SharePoint!.Mode.Should().Be("simulated");
            evidence.SharePoint.Operations.Should().NotBeEmpty();
            evidence.Erp.Should().NotBeNull();
            evidence.Erp!.Status.Should().Be("Received");
            evidence.Erp.ExternalReceiptId.Should().Be(run.Result.ErpReceiptId);
            evidence.Erp.Attempts.Should().Be(1);
            evidence.Links.CaseHref.Should().Be(run.Result.CaseHref);
            evidence.Links.DocumentHref.Should().Be(run.Result.DocumentHref);
            evidence.Links.DownloadHref.Should().Be(run.Result.DocumentDownloadHref);

            var caseId = Guid.Parse(evidence.Links.CaseHref!.Split('/').Last());
            using var caseResponse = await SendAuthenticatedAsync(
                client, HttpMethod.Get, $"/api/cases/{caseId}", session.Token);
            caseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var openedCase = await caseResponse.Content.ReadFromJsonAsync<CaseResponse>(
                TestContext.Current.CancellationToken);
            openedCase!.CaseNumber.Should().Be(evidence.Case!.CaseNumber);

            using var documentResponse = await SendAuthenticatedAsync(
                client, HttpMethod.Get, evidence.Links.DownloadHref!, session.Token);
            documentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            documentResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
            var documentBytes = await documentResponse.Content.ReadAsByteArrayAsync(
                TestContext.Current.CancellationToken);
            documentBytes.Should().StartWith("%PDF"u8.ToArray());

            using var receiverScope = receiverFactory.Services.CreateScope();
            var receiverDb = receiverScope.ServiceProvider.GetRequiredService<ErpReceiverDbContext>();
            var receipt = await receiverDb.Receipts.AsNoTracking().SingleAsync(
                candidate => candidate.ExternalReceiptId == run.Result.ErpReceiptId,
                TestContext.Current.CancellationToken);
            receipt.ExternalReceiptId.Should().Be(run.Result.ErpReceiptId);
            receipt.CaseNumber.Should().Be(evidence.Case.CaseNumber);
            receipt.DocumentReference.Should().Be(evidence.Document!.FileName);
            receipt.AttemptCount.Should().Be(1);
        }
        finally
        {
            File.Delete(receiverDatabase);
            File.Delete($"{receiverDatabase}-shm");
            File.Delete($"{receiverDatabase}-wal");
        }
    }

    [Fact]
    public async Task Fail_once_retry_preserves_artifacts_and_creates_one_idempotent_receipt()
    {
        var receiverDatabase = Path.Combine(Path.GetTempPath(), $"norvixhub-erp-retry-e2e-{Guid.NewGuid():N}.db");
        try
        {
            using var receiverFactory = new WebApplicationFactory<ErpReceiverProgram>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Demo");
                    builder.ConfigureAppConfiguration(configuration =>
                        configuration.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:ErpDemoReceiver"] = $"Data Source={receiverDatabase}",
                            ["ErpDemoReceiver:SigningSecret"] = SigningSecret,
                            ["ErpDemoReceiver:EnableFailOnce"] = "true"
                        }));
                });
            using var receiverClient = receiverFactory.CreateClient();
            using var factory = CreateApplicationFactory(receiverClient);
            using var client = factory.CreateClient();

            var session = await CreateDemoSessionAsync(client);
            var created = await CreateRunAsync(client, session.Token, simulateErpFailureOnce: true);
            await ProcessAsync(factory, created.RunId);

            var failedRun = await GetRunAsync(client, session.Token, created.RunId);
            failedRun.Status.Should().Be("Failed");
            failedRun.CanRetry.Should().BeTrue();
            var failedEvidence = await GetEvidenceAsync(client, session.Token, created.RunId);
            failedEvidence.Case.Should().NotBeNull();
            failedEvidence.Document.Should().NotBeNull();
            failedEvidence.SharePoint.Should().NotBeNull();
            failedEvidence.Erp.Should().NotBeNull();
            failedEvidence.Erp!.Status.Should().Be("Failed");
            failedEvidence.Erp.Attempts.Should().Be(1);

            await RetryRunAsync(client, session.Token, created.RunId);
            await ProcessAsync(factory, created.RunId);

            var completedRun = await GetRunAsync(client, session.Token, created.RunId);
            completedRun.Status.Should().Be("Completed");
            completedRun.RetryCount.Should().Be(1);
            completedRun.Result!.ErpReceiptId.Should().StartWith("ERP-DEMO-");
            var completedEvidence = await GetEvidenceAsync(client, session.Token, created.RunId);
            completedEvidence.Run.Status.Should().Be("Completed");
            completedEvidence.Case!.CaseNumber.Should().Be(failedEvidence.Case!.CaseNumber);
            completedEvidence.Case.CaseHref.Should().Be(failedEvidence.Case.CaseHref);
            completedEvidence.Document!.DocumentId.Should().Be(failedEvidence.Document!.DocumentId);
            completedEvidence.Document.ContentHash.Should().Be(failedEvidence.Document.ContentHash);
            completedEvidence.SharePoint!.FolderId.Should().Be(failedEvidence.SharePoint!.FolderId);
            completedEvidence.SharePoint.FileId.Should().Be(failedEvidence.SharePoint.FileId);
            completedEvidence.Erp!.Status.Should().Be("Received");
            completedEvidence.Erp.ExternalReceiptId.Should().Be(completedRun.Result.ErpReceiptId);
            completedEvidence.Erp.Attempts.Should().Be(2);
            completedEvidence.Erp.History.Select(attempt => attempt.Status)
                .Should().Equal("Failed", "Received");
            completedEvidence.AuditEvents.Should().Contain(audit =>
                audit.EventType == "LiveDemoStepFailed" && audit.Provider == "ERP demo receiver");
            completedEvidence.AuditEvents.Should().Contain(audit =>
                audit.EventType == "LiveDemoRunRetried");

            using var receiverScope = receiverFactory.Services.CreateScope();
            var receiverDb = receiverScope.ServiceProvider.GetRequiredService<ErpReceiverDbContext>();
            var receipts = await receiverDb.Receipts.AsNoTracking()
                .Where(receipt => receipt.IdempotencyKey == $"live-demo-{created.RunId:N}")
                .ToListAsync(TestContext.Current.CancellationToken);
            receipts.Should().ContainSingle();
            receipts[0].ExternalReceiptId.Should().Be(completedRun.Result.ErpReceiptId);
            receipts[0].AttemptCount.Should().Be(2);
            receipts[0].FailOnceTriggered.Should().BeTrue();
        }
        finally
        {
            File.Delete(receiverDatabase);
            File.Delete($"{receiverDatabase}-shm");
            File.Delete($"{receiverDatabase}-wal");
        }
    }

    private WebApplicationFactory<Program> CreateApplicationFactory(HttpClient receiverClient) =>
        apiFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(configuration =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777",
                    ["ErpDemo:Enabled"] = "true",
                    ["ErpDemo:FailureDemoEnabled"] = "true"
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IErpDemoClient>();
                services.AddSingleton<IErpDemoClient>(_ => new ErpDemoClient(
                    receiverClient,
                    Options.Create(new ErpDemoOptions
                    {
                        BaseUrl = receiverClient.BaseAddress!.ToString(),
                        SigningSecret = SigningSecret,
                        Enabled = true
                    })));
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

    private static Task<CreateLiveDemoRunResponse> CreateRunAsync(HttpClient client, string token) =>
        CreateRunAsync(client, token, simulateErpFailureOnce: false);

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

    private static async Task RetryRunAsync(HttpClient client, string token, Guid runId)
    {
        using var response = await SendAuthenticatedAsync(
            client, HttpMethod.Post, $"/api/live-demo-runs/{runId}/retry", token);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    private static async Task ProcessAsync(WebApplicationFactory<Program> factory, Guid runId)
    {
        using var scope = factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<ILiveDemoRunProcessor>();
        await processor.ProcessAsync(runId, TestContext.Current.CancellationToken);
    }

    private static async Task<LiveDemoRunResponse> GetRunAsync(
        HttpClient client, string token, Guid runId)
    {
        using var response = await SendAuthenticatedAsync(
            client, HttpMethod.Get, $"/api/live-demo-runs/{runId}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<LiveDemoRunResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<LiveDemoEvidenceResponse> GetEvidenceAsync(
        HttpClient client, string token, Guid runId)
    {
        using var response = await SendAuthenticatedAsync(
            client, HttpMethod.Get, $"/api/live-demo-runs/{runId}/evidence", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<LiveDemoEvidenceResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpClient client, HttpMethod method, string path, string token)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
