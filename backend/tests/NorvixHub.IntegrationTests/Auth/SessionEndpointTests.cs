using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Auth;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Auth;

public sealed class SessionEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public SessionEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Api_endpoint_without_local_dev_headers_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/me",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Current_user_returns_demo_membership()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        DevAuthHeaders.AddDemoAdmin(request);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(
            TestContext.Current.CancellationToken);
        body.Should().NotBeNull();
        body!.TenantId.Should().Be(LocalDevTenantContext.DemoTenantId);
        body.UserId.Should().Be(LocalDevTenantContext.DemoUserId);
        body.Role.Should().Be("TenantOwner");
    }

    [Fact]
    public async Task User_cannot_authenticate_against_tenant_without_membership()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        DevAuthHeaders.Add(
            request,
            NorvixHubApiFactory.SecondTenantId,
            LocalDevTenantContext.DemoUserId);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Disabled_user_cannot_authenticate_even_with_membership()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        DevAuthHeaders.Add(
            request,
            LocalDevTenantContext.DemoTenantId,
            NorvixHubApiFactory.DisabledUserId);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tenant_list_only_returns_current_users_memberships()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tenants");
        DevAuthHeaders.AddDemoAdmin(request);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tenants = await response.Content.ReadFromJsonAsync<List<TenantSummaryResponse>>(
            TestContext.Current.CancellationToken);
        tenants.Should().ContainSingle();
        tenants![0].TenantId.Should().Be(LocalDevTenantContext.DemoTenantId);
    }

    [Fact]
    public async Task Switch_tenant_rejects_tenant_without_membership()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/tenants/{NorvixHubApiFactory.SecondTenantId}/switch");
        DevAuthHeaders.AddDemoAdmin(request);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

