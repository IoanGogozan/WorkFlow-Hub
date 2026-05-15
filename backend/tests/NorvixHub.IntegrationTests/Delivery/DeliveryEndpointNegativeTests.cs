using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Delivery;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Delivery;

public sealed partial class DeliveryEndpointTests
{
    [Fact]
    public async Task Invalid_delivery_token_returns_not_found()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/delivery/random-token", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Expired_delivery_link_returns_gone()
    {
        using var client = _factory.CreateClient();
        var package = await CreateReadyPackageAsync(client);
        var token = await _factory.CreateExpiredDeliveryTokenAsync(package.Id);

        using var response = await client.GetAsync($"/delivery/{token}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Package_cannot_include_document_from_another_tenant()
    {
        await _factory.SeedExtraTenantsAsync();
        var otherTenantDocumentId = await _factory.CreateSecondTenantDocumentAsync();
        using var client = _factory.CreateClient();
        var caseWorkspace = await CreateCaseAsync(client);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/cases/{caseWorkspace.Id}/delivery-packages",
            new CreateDeliveryPackageRequest("Unsafe package", [otherTenantDocumentId]));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Viewer_cannot_create_delivery_package()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        var caseWorkspace = await CreateCaseAsync(client);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/cases/{caseWorkspace.Id}/delivery-packages")
        {
            Content = JsonContent.Create(new CreateDeliveryPackageRequest("Viewer package", [Guid.NewGuid()]))
        };
        DevAuthHeaders.Add(request, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
