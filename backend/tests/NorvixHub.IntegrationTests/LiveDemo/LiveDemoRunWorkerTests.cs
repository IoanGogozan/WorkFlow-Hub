using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Contracts.Auth;
using NorvixHub.Contracts.LiveDemo;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Infrastructure.LiveDemo;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using NorvixHub.Worker;
using Xunit;

namespace NorvixHub.IntegrationTests.LiveDemo;

public sealed class LiveDemoRunWorkerTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public LiveDemoRunWorkerTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Worker_processes_one_queued_run_when_polled()
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
                    ["LiveDemo:WorkerPollMilliseconds"] = "100",
                    ["LiveDemo:RunRecoveryMinutes"] = "5"
                });
            });
        });
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, session.Token);
        var worker = new LiveDemoRunWorker(
            factory.Services.GetRequiredService<IServiceScopeFactory>(),
            factory.Services.GetRequiredService<IOptions<LiveDemoOptions>>(),
            NullLogger<LiveDemoRunWorker>.Instance);

        await worker.RunOnceAsync(TestContext.Current.CancellationToken);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        (await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == run.RunId,
            TestContext.Current.CancellationToken)).Status.Should().Be(LiveDemoRunStatus.Completed);
    }

    [Fact(Skip = "ERP retry is disabled until the receiver capability is active.")]
    public async Task New_worker_instance_resumes_ERP_retry_after_controlled_failure()
    {
        var erpClient = new RestartErpDemoClient();
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LiveDemo:Enabled"] = "true",
                    ["LiveDemo:OrganizationNumber"] = "999888777",
                    ["LiveDemo:WorkerPollMilliseconds"] = "100",
                    ["LiveDemo:RunRecoveryMinutes"] = "5",
                    ["ErpDemo:Enabled"] = "true"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IErpDemoClient>();
                services.AddSingleton<IErpDemoClient>(erpClient);
            });
        });
        using var client = factory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        var run = await CreateRunAsync(client, session.Token, simulateErpFailureOnce: true);
        var firstWorker = CreateWorker(factory);

        await firstWorker.RunOnceAsync(TestContext.Current.CancellationToken);
        await RetryRunAsync(client, run.RunId, session.Token);
        var restartedWorker = CreateWorker(factory);
        await restartedWorker.RunOnceAsync(TestContext.Current.CancellationToken);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var persistedRun = await dbContext.LiveDemoRuns.SingleAsync(
            candidate => candidate.Id == run.RunId,
            TestContext.Current.CancellationToken);
        persistedRun.Status.Should().Be(LiveDemoRunStatus.Completed);
        persistedRun.ErpReceiptId.Should().Be("ERP-DEMO-WORKER01");
        erpClient.Requests.Should().HaveCount(2);
    }

    private static LiveDemoRunWorker CreateWorker(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory) => new(
        factory.Services.GetRequiredService<IServiceScopeFactory>(),
        factory.Services.GetRequiredService<IOptions<LiveDemoOptions>>(),
        NullLogger<LiveDemoRunWorker>.Instance);

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

    private sealed class RestartErpDemoClient : IErpDemoClient
    {
        public List<ErpDemoRequest> Requests { get; } = [];

        public Task<ErpDemoResult> SendAsync(ErpDemoRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(Requests.Count == 1
                ? new ErpDemoResult(ErpDemoResultStatus.Unavailable)
                : new ErpDemoResult(ErpDemoResultStatus.Received, "ERP-DEMO-WORKER01", false, DateTime.UtcNow));
        }
    }
}
