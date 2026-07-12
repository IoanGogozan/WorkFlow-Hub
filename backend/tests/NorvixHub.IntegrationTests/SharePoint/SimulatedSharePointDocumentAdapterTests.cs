using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.SharePoint;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.SharePoint;

public sealed class SimulatedSharePointDocumentAdapterTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public SimulatedSharePointDocumentAdapterTests(NorvixHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Repeated_document_version_synchronization_creates_one_external_item()
    {
        using var scope = _factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var tenantId = Guid.NewGuid();
        var request = new SharePointDocumentSyncRequest(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), "Fjord Pumpeteknikk AS", "CASE-2026-0014",
            Guid.NewGuid(), Guid.NewGuid(), "service-request.pdf", 238144, "ServiceRequest", "Approved");

        var first = await adapter.SynchronizeAsync(request, TestContext.Current.CancellationToken);
        var second = await adapter.SynchronizeAsync(request, TestContext.Current.CancellationToken);

        first.Succeeded.Should().BeTrue();
        first.AlreadySynchronized.Should().BeFalse();
        second.Succeeded.Should().BeTrue();
        second.AlreadySynchronized.Should().BeTrue();
        (await dbContext.SimulatedSharePointDocumentItems.CountAsync(item => item.TenantId == tenantId, TestContext.Current.CancellationToken)).Should().Be(1);
        (await dbContext.SimulatedSharePointOperations.CountAsync(operation => operation.TenantId == tenantId, TestContext.Current.CancellationToken)).Should().Be(6);
    }

    [Fact]
    public async Task New_version_updates_etag_and_stale_etag_is_rejected()
    {
        using var scope = _factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var tenantId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var first = new SharePointDocumentSyncRequest(tenantId, null, Guid.NewGuid(), "Fjord Pumpeteknikk AS", "CASE-2026-0015", documentId, Guid.NewGuid(), "report.pdf", 100, "Report", "Approved");
        var created = await adapter.SynchronizeAsync(first, TestContext.Current.CancellationToken);
        var updated = await adapter.SynchronizeAsync(first with { DocumentVersionId = Guid.NewGuid(), ExpectedETag = created.Item!.ETag }, TestContext.Current.CancellationToken);
        var stale = await adapter.SynchronizeAsync(first with { DocumentVersionId = Guid.NewGuid(), ExpectedETag = created.Item!.ETag }, TestContext.Current.CancellationToken);

        updated.Succeeded.Should().BeTrue();
        updated.Item!.ExternalItemId.Should().Be(created.Item.ExternalItemId);
        updated.Item.ETag.Should().NotBe(created.Item.ETag);
        updated.Item.Version.Should().Be("2.0");
        stale.Succeeded.Should().BeFalse();
        stale.StatusCode.Should().Be(412);
        stale.ErrorCode.Should().Be("PRECONDITION_FAILED");
    }

    [Fact]
    public async Task Only_configured_site_is_allowed_and_operations_remain_tenant_scoped()
    {
        using var scope = _factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var firstTenant = Guid.NewGuid();
        var secondTenant = Guid.NewGuid();

        var allowed = await adapter.TestSiteAccessAsync(firstTenant, "site-demo-service", TestContext.Current.CancellationToken);
        var denied = await adapter.TestSiteAccessAsync(firstTenant, "site-hr-internal", TestContext.Current.CancellationToken);
        await adapter.TestSiteAccessAsync(secondTenant, "site-hr-internal", TestContext.Current.CancellationToken);

        allowed.Succeeded.Should().BeTrue();
        denied.Succeeded.Should().BeFalse();
        denied.StatusCode.Should().Be(403);
        denied.ErrorCode.Should().Be("accessDenied");
        (await dbContext.SimulatedSharePointOperations.CountAsync(operation => operation.TenantId == firstTenant, TestContext.Current.CancellationToken)).Should().Be(2);
        (await dbContext.SimulatedSharePointOperations.CountAsync(operation => operation.TenantId == secondTenant, TestContext.Current.CancellationToken)).Should().Be(1);
    }
}
