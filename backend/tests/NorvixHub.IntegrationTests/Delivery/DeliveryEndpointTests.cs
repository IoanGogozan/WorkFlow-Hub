using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Contracts.Cases;
using NorvixHub.Contracts.Delivery;
using NorvixHub.Contracts.Documents;
using NorvixHub.Contracts.Intake;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Delivery;

public sealed partial class DeliveryEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public DeliveryEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Delivery_package_flow_creates_link_and_logs_public_access()
    {
        using var client = _factory.CreateClient();
        var caseWorkspace = await CreateCaseAsync(client);
        var document = await UploadAndLinkDocumentAsync(client, caseWorkspace.Id, "handover.pdf");
        var package = await CreatePackageAsync(client, caseWorkspace.Id, [document.Id]);

        var generated = await PostJsonAsync<DeliveryPackageResponse>(
            client,
            $"/api/delivery-packages/{package.Id}/generate-pdf");
        generated.Status.Should().Be("Ready");
        generated.SummaryPdfDocumentId.Should().NotBeNull();

        var linked = await CreateLinkAsync(client, package.Id, DateTimeOffset.UtcNow.AddDays(7));
        var token = linked.Links.Single(link => link.Token is not null).Token!;
        var logCountBefore = await _factory.CountDeliveryAccessLogsAsync(package.Id);

        var publicPackage = await GetPublicJsonAsync<PublicDeliveryPackageResponse>(client, $"/delivery/{token}");

        publicPackage.Title.Should().Be(package.Title);
        publicPackage.Documents.Should().ContainSingle(item => item.DocumentId == document.Id);
        var logCountAfter = await _factory.CountDeliveryAccessLogsAsync(package.Id);
        logCountAfter.Should().Be(logCountBefore + 1);
    }

    [Fact]
    public async Task Delivery_link_only_exposes_selected_documents()
    {
        using var client = _factory.CreateClient();
        var caseWorkspace = await CreateCaseAsync(client);
        var selected = await UploadAndLinkDocumentAsync(client, caseWorkspace.Id, "selected.pdf");
        var notSelected = await UploadAndLinkDocumentAsync(client, caseWorkspace.Id, "internal.pdf");
        var package = await CreatePackageAsync(client, caseWorkspace.Id, [selected.Id]);
        var linked = await CreateLinkAsync(client, package.Id, DateTimeOffset.UtcNow.AddDays(2));
        var token = linked.Links.Single(link => link.Token is not null).Token!;

        var publicPackage = await GetPublicJsonAsync<PublicDeliveryPackageResponse>(client, $"/delivery/{token}");
        using var blocked = await client.GetAsync(
            $"/delivery/{token}/documents/{notSelected.Id}",
            TestContext.Current.CancellationToken);

        publicPackage.Documents.Should().ContainSingle(item => item.DocumentId == selected.Id);
        blocked.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Public_document_access_is_logged()
    {
        using var client = _factory.CreateClient();
        var caseWorkspace = await CreateCaseAsync(client);
        var document = await UploadAndLinkDocumentAsync(client, caseWorkspace.Id, "public.pdf");
        var package = await CreatePackageAsync(client, caseWorkspace.Id, [document.Id]);
        var linked = await CreateLinkAsync(client, package.Id, DateTimeOffset.UtcNow.AddDays(1));
        var token = linked.Links.Single(link => link.Token is not null).Token!;
        var logCountBefore = await _factory.CountDeliveryAccessLogsAsync(package.Id);

        using var response = await client.GetAsync(
            $"/delivery/{token}/documents/{document.Id}",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logCountAfter = await _factory.CountDeliveryAccessLogsAsync(package.Id);
        logCountAfter.Should().Be(logCountBefore + 1);
    }

    [Fact]
    public async Task Revoked_delivery_link_returns_gone()
    {
        using var client = _factory.CreateClient();
        var package = await CreateReadyPackageAsync(client);
        var linked = await CreateLinkAsync(client, package.Id, DateTimeOffset.UtcNow.AddDays(3));
        var link = linked.Links.Single(item => item.Token is not null);

        using var revokeResponse = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/delivery-links/{link.Id}/revoke");
        using var publicResponse = await client.GetAsync($"/delivery/{link.Token}", TestContext.Current.CancellationToken);

        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.Gone);
    }
}
