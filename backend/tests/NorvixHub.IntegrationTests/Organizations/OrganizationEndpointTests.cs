using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Customers;
using NorvixHub.Contracts.Organizations;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Organizations;

public sealed class OrganizationEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public OrganizationEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Search_organizations_returns_brreg_results()
    {
        using var client = _factory.CreateClient();

        var results = await GetJsonAsync<List<OrganizationSearchResultResponse>>(
            client,
            "/api/organizations/search?query=sordal");

        results.Should().ContainSingle();
        results[0].OrganizationNumber.Should().Be("999888777");
    }

    [Fact]
    public async Task Get_organization_by_org_number_returns_result()
    {
        using var client = _factory.CreateClient();

        var result = await GetJsonAsync<OrganizationSearchResultResponse>(
            client,
            "/api/organizations/brreg/999888777");

        result.Name.Should().Be("Sordal Eiendom AS");
    }

    [Fact]
    public async Task Create_customer_from_brreg_is_idempotent_and_writes_audit()
    {
        using var client = _factory.CreateClient();
        var auditCountBefore = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "Customer",
            "CustomerCreatedFromBrreg");
        var request = new CreateCustomerFromBrregRequest("999888777");

        using var firstResponse = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            "/api/customers/from-brreg",
            request);
        using var secondResponse = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            "/api/customers/from-brreg",
            request);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var first = await firstResponse.Content.ReadFromJsonAsync<CustomerResponse>(
            TestContext.Current.CancellationToken);
        var second = await secondResponse.Content.ReadFromJsonAsync<CustomerResponse>(
            TestContext.Current.CancellationToken);
        second!.Id.Should().Be(first!.Id);

        var auditCountAfter = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "Customer",
            "CustomerCreatedFromBrreg");
        auditCountAfter.Should().Be(auditCountBefore + 1);
    }

    [Fact]
    public async Task Invalid_org_number_returns_bad_request()
    {
        using var client = _factory.CreateClient();

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Get,
            "/api/organizations/brreg/ABC");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unknown_org_number_returns_not_found()
    {
        using var client = _factory.CreateClient();

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Get,
            "/api/organizations/brreg/111222333");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Viewer_cannot_create_customer_from_brreg()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/customers/from-brreg")
        {
            Content = JsonContent.Create(new CreateCustomerFromBrregRequest("999888777"))
        };
        DevAuthHeaders.Add(request, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Search_without_auth_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/organizations/search?query=sordal",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

