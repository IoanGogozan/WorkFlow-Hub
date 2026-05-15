using System.Net;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Documents;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Documents;

public sealed partial class DocumentEndpointTests
{
    [Fact]
    public async Task Unsupported_file_extension_is_rejected()
    {
        using var client = _factory.CreateClient();

        using var response = await SendMultipartAsync(
            client,
            HttpMethod.Post,
            "/api/documents",
            "payload.exe",
            "application/octet-stream");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Mime_mismatch_is_rejected()
    {
        using var client = _factory.CreateClient();

        using var response = await SendMultipartAsync(client, HttpMethod.Post, "/api/documents", "demo.pdf", "text/plain");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Viewer_cannot_upload_document()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        using var request = CreateMultipartRequest(HttpMethod.Post, "/api/documents", "demo.pdf", "application/pdf");
        DevAuthHeaders.Add(request, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Document_from_another_tenant_returns_not_found()
    {
        await _factory.SeedExtraTenantsAsync();
        var otherTenantDocumentId = await _factory.CreateSecondTenantDocumentAsync();
        using var client = _factory.CreateClient();

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Get,
            $"/api/documents/{otherTenantDocumentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Linking_document_to_case_from_another_tenant_returns_not_found()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        var document = await UploadDocumentAsync(client);
        var otherTenantCaseId = await _factory.CreateSecondTenantCaseAsync();

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/documents/{document.Id}/link-to-case",
            new LinkDocumentToCaseRequest(otherTenantCaseId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
