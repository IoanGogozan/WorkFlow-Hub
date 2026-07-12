using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.SharePoint;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.SharePoint;

public sealed class SharePointTechnicalEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public SharePointTechnicalEndpointTests(NorvixHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Admin_can_read_status_and_only_current_tenant_documents()
    {
        await _factory.SeedExtraTenantsAsync();
        var ownName = "own-" + Guid.NewGuid().ToString("N") + ".pdf";
        var otherName = "other-" + Guid.NewGuid().ToString("N") + ".pdf";
        await SeedItemAsync(LocalDevTenantContext.DemoTenantId, ownName);
        await SeedItemAsync(NorvixHubApiFactory.SecondTenantId, otherName);
        using var client = _factory.CreateClient();

        using var statusResponse = await SendAsync(client, HttpMethod.Get, "/api/technical/sharepoint/status");
        using var documentsResponse = await SendAsync(client, HttpMethod.Get, "/api/technical/sharepoint/documents");

        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusJson = await statusResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        statusJson.Should().Contain("Simulated").And.Contain("No Microsoft 365 tenant is connected");
        var documentsJson = await documentsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        documentsJson.Should().Contain(ownName).And.NotContain(otherName);
    }

    [Fact]
    public async Task Viewer_cannot_access_technical_sharepoint_endpoints()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/technical/sharepoint/operations");
        DevAuthHeaders.Add(request, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Restricted_access_action_returns_safe_simulated_403_evidence()
    {
        using var client = _factory.CreateClient();

        using var response = await SendAsync(client, HttpMethod.Post, "/api/technical/sharepoint/test-restricted-access");
        var result = await response.Content.ReadFromJsonAsync<AccessResponse>(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.ErrorCode.Should().Be("accessDenied");
    }

    private async Task SeedItemAsync(Guid tenantId, string name)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var documentId = Guid.NewGuid();
        db.SimulatedSharePointDocumentItems.Add(new SimulatedSharePointDocumentItem
        {
            TenantId = tenantId,
            SiteId = "site-demo-service",
            DriveId = "drive-shared-documents",
            DocumentId = documentId,
            DocumentVersionId = Guid.NewGuid(),
            ExternalItemId = "01SP-DEMO-" + documentId.ToString("N")[..10],
            ParentPath = "/Shared Documents/Customers/Test/CASE/Incoming",
            Name = name,
            ETag = "demo-etag-1",
            Version = "1.0",
            MetadataJson = "{}",
            SyncStatus = "Synchronized",
            IdempotencyKey = tenantId.ToString("N") + ":" + documentId.ToString("N")
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        DevAuthHeaders.AddDemoAdmin(request);
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private sealed record AccessResponse(bool Succeeded, int StatusCode, string? ErrorCode, string PublicMessage);
}
