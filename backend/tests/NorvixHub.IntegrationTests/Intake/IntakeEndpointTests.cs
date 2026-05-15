using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Intake;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Intake;

public sealed class IntakeEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public IntakeEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_intake_returns_created_item_and_writes_audit_event()
    {
        using var client = _factory.CreateClient();
        var auditCountBefore = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "IntakeItem",
            "IntakeCreated");
        using var request = CreateDemoRequest(CreateRequest());

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken);
        body.Should().NotBeNull();
        body!.TenantId.Should().Be(LocalDevTenantContext.DemoTenantId);
        body.Status.Should().Be("New");

        var auditCountAfter = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "IntakeItem",
            "IntakeCreated");
        auditCountAfter.Should().Be(auditCountBefore + 1);
    }

    [Fact]
    public async Task List_intakes_only_returns_current_tenant_items()
    {
        await _factory.SeedExtraTenantsAsync();
        var otherTenantIntake = await _factory.CreateSecondTenantIntakeAsync();
        using var client = _factory.CreateClient();
        using var createRequest = CreateDemoRequest(CreateRequest());
        using var createResponse = await client.SendAsync(
            createRequest,
            TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/intakes");
        DevAuthHeaders.AddDemoAdmin(listRequest);
        using var listResponse = await client.SendAsync(
            listRequest,
            TestContext.Current.CancellationToken);

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var intakes = await listResponse.Content.ReadFromJsonAsync<List<IntakeListItemResponse>>(
            TestContext.Current.CancellationToken);
        intakes.Should().NotBeNull();
        intakes.Should().NotContain(intake => intake.Subject == otherTenantIntake.Subject);
    }

    [Fact]
    public async Task Get_intake_returns_item_for_current_tenant()
    {
        using var client = _factory.CreateClient();
        using var createRequest = CreateDemoRequest(CreateRequest());
        using var createResponse = await client.SendAsync(
            createRequest,
            TestContext.Current.CancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/intakes/{created!.Id}");
        DevAuthHeaders.AddDemoAdmin(getRequest);
        using var getResponse = await client.SendAsync(
            getRequest,
            TestContext.Current.CancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken);
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Get_intake_from_another_tenant_returns_not_found()
    {
        await _factory.SeedExtraTenantsAsync();
        var otherTenantIntake = await _factory.CreateSecondTenantIntakeAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/intakes/{otherTenantIntake.Id}");
        DevAuthHeaders.AddDemoAdmin(request);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_intake_without_auth_returns_unauthorized()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/intakes")
        {
            Content = JsonContent.Create(CreateRequest())
        };

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Viewer_cannot_create_intake()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/intakes")
        {
            Content = JsonContent.Create(CreateRequest())
        };
        DevAuthHeaders.Add(
            request,
            LocalDevTenantContext.DemoTenantId,
            NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_intake_rejects_invalid_source()
    {
        using var client = _factory.CreateClient();
        using var request = CreateDemoRequest(CreateRequest(Source: "RealEmail"));

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_intake_rejects_missing_subject()
    {
        using var client = _factory.CreateClient();
        using var request = CreateDemoRequest(CreateRequest(Subject: " "));

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static HttpRequestMessage CreateDemoRequest(CreateIntakeRequest body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/intakes")
        {
            Content = JsonContent.Create(body)
        };
        DevAuthHeaders.AddDemoAdmin(request);
        return request;
    }

    private static CreateIntakeRequest CreateRequest(
        string Source = "Manual",
        string Subject = "New service request",
        string Body = "Customer asks for operational documentation follow-up.")
    {
        var subject = string.IsNullOrWhiteSpace(Subject) ? Subject : $"{Subject} {Guid.NewGuid():N}";

        return new CreateIntakeRequest(
            Source,
            subject,
            Body,
            "Sordal Eiendom AS",
            "999888777",
            "Documentation",
            "Normal");
    }
}
