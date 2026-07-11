using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Contracts.Auth;
using NorvixHub.Contracts.DemoStory;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.DemoStory;

public sealed class DemoStoryEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public DemoStoryEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DemoStory_returns_coherent_tenant_scoped_seeded_scenario()
    {
        using var demoFactory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Demo"));
        using var client = demoFactory.CreateClient();
        using var createResponse = await client.PostAsync(
            "/api/demo-sessions",
            content: null,
            TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await createResponse.Content.ReadFromJsonAsync<CreateDemoSessionResponse>(
            TestContext.Current.CancellationToken);
        session.Should().NotBeNull();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/demo-story");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session!.Token);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var story = await response.Content.ReadFromJsonAsync<DemoStoryResponse>(
            TestContext.Current.CancellationToken);
        story.Should().NotBeNull();
        story!.ScenarioKey.Should().Be("pump-station-service");
        story.Request.Sender.Should().Be("service@kristiansand.example.test");
        story.Request.Subject.Should().Be("Service og dokumentasjon – pumpestasjon 14");
        story.Request.Body.Should().Contain("Kundereferanse: PO-10482");
        story.Request.CustomerReference.Should().Be("PO-10482");
        story.Request.Attachments.Should().BeEquivalentTo(
            "inspeksjonsnotat.pdf",
            "bilder-pumpestasjon-14.zip");
        story.Outcome.CaseTitle.Should().Be("Service og dokumentasjon – pumpestasjon 14");
        story.Outcome.DeliveryPackageTitle.Should().Be("Leveringsgrunnlag – pumpestasjon 14");
        story.EvidenceSteps.Should().HaveCount(8);
        story.EvidenceSteps.Select(step => step.Sequence).Should().Equal(1, 2, 3, 4, 5, 6, 7, 8);
        story.Outcome.LinkedDocumentCount.Should().BeGreaterThan(0);
        story.Outcome.AuditEventCount.Should().BeGreaterThan(0);
        story.TechnicalLinks.PrimaryDocumentHref.Should().NotBeNullOrWhiteSpace();
        story.TechnicalLinks.DeliveryPackageHref.Should().NotBeNullOrWhiteSpace();
        story.EvidenceSteps.Should().Contain(step => step.EvidenceMode == "implemented");
        story.EvidenceSteps.Should().Contain(step => step.EvidenceMode == "demo-adapter");
        story.EvidenceSteps.Should().ContainSingle(step =>
            step.Key == "email-received" &&
            step.System == "Fiktiv e-postkilde" &&
            step.EvidenceMode == "scenario-source");
        story.EvidenceSteps.Should().ContainSingle(step =>
            step.Key == "case-created" &&
            step.System == "Norvix demoarbeidsflyt" &&
            step.EvidenceMode == "implemented");
        story.EvidenceSteps.Should().ContainSingle(step =>
            step.Key == "delivery-created" && step.EvidenceMode == "implemented");
        story.EvidenceSteps.Should().ContainSingle(step =>
            step.Key == "reporting-simulated" && step.EvidenceMode == "demo-adapter");
        story.Integrations.Should().Contain(integration => integration.Mode == "public-data-capable");
        story.Integrations.Should().Contain(integration => integration.Mode == "demo-adapter");
        story.Integrations
            .Where(integration => integration.Provider is "microsoft-graph" or "tripletex" or "powerbi-fabric")
            .Should().OnlyContain(integration =>
                integration.Status == "Disconnected" &&
                integration.Explanation.Contains("ingen ekte tilkobling"));
        response.Headers.GetValues("X-Correlation-ID").Single().Should().NotBeNullOrWhiteSpace();
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");

        using var scope = demoFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var caseId = Guid.Parse(story.TechnicalLinks.CaseHref.Split('/').Last());
        var intakeId = Guid.Parse(story.TechnicalLinks.IntakeHref.Split('/').Last());
        (await dbContext.Cases.AnyAsync(
            candidate => candidate.Id == caseId && candidate.TenantId == session.DemoTenantId,
            TestContext.Current.CancellationToken)).Should().BeTrue();
        (await dbContext.IntakeItems.AnyAsync(
            candidate => candidate.Id == intakeId && candidate.TenantId == session.DemoTenantId,
            TestContext.Current.CancellationToken)).Should().BeTrue();

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.Should().NotContain("settingsJson");
        json.Should().NotContain("tokenHash");
        json.Should().NotContain("ipAddress");
        json.Should().NotContain("blobName");
    }

    [Fact]
    public async Task DemoStory_cannot_expose_another_demo_sessions_story()
    {
        using var demoFactory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Demo"));
        using var client = demoFactory.CreateClient();
        var firstSession = await CreateDemoSessionAsync(client);
        var secondSession = await CreateDemoSessionAsync(client);

        var firstStory = await GetDemoStoryAsync(client, firstSession.Token);
        var secondStory = await GetDemoStoryAsync(client, secondSession.Token);

        firstStory.TechnicalLinks.CaseHref.Should().NotBe(secondStory.TechnicalLinks.CaseHref);
        firstStory.TechnicalLinks.IntakeHref.Should().NotBe(secondStory.TechnicalLinks.IntakeHref);
        var firstJson = JsonSerializer.Serialize(firstStory);
        firstJson.Should().NotContain(secondStory.TechnicalLinks.CaseHref);
        firstJson.Should().NotContain(secondStory.TechnicalLinks.IntakeHref);

        using var scope = demoFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var firstCaseId = IdFromHref(firstStory.TechnicalLinks.CaseHref);
        var secondCaseId = IdFromHref(secondStory.TechnicalLinks.CaseHref);
        (await dbContext.Cases.SingleAsync(
            candidate => candidate.Id == firstCaseId,
            TestContext.Current.CancellationToken)).TenantId.Should().Be(firstSession.DemoTenantId);
        (await dbContext.Cases.SingleAsync(
            candidate => candidate.Id == secondCaseId,
            TestContext.Current.CancellationToken)).TenantId.Should().Be(secondSession.DemoTenantId);
    }

    [Fact]
    public async Task DemoStory_returns_stable_not_found_when_scenario_is_missing()
    {
        using var demoFactory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Demo"));
        using var client = demoFactory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        using (var scope = demoFactory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
            await dbContext.Cases
                .Where(candidate => candidate.TenantId == session.DemoTenantId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(candidate => candidate.SourceIntakeItemId, (Guid?)null),
                    TestContext.Current.CancellationToken);
        }

        using var request = AuthorizedRequest(session.Token);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(
            TestContext.Current.CancellationToken);
        error.Should().Contain("error", "Demo story is not available.");
    }

    [Fact]
    public async Task DemoStory_rejects_unauthenticated_public_request()
    {
        using var demoFactory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Demo"));
        using var client = demoFactory.CreateClient();

        using var response = await client.GetAsync(
            "/api/demo-story",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(
            TestContext.Current.CancellationToken);
        error.Should().Contain("error", "Demo session bearer token is required.");
        response.Headers.GetValues("X-Correlation-ID").Single().Should().NotBeNullOrWhiteSpace();
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
    }

    private static async Task<CreateDemoSessionResponse> CreateDemoSessionAsync(HttpClient client)
    {
        using var response = await client.PostAsync(
            "/api/demo-sessions",
            content: null,
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateDemoSessionResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<DemoStoryResponse> GetDemoStoryAsync(HttpClient client, string token)
    {
        using var request = AuthorizedRequest(token);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DemoStoryResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static HttpRequestMessage AuthorizedRequest(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/demo-story");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static Guid IdFromHref(string href) => Guid.Parse(href.Split('/').Last());
}
