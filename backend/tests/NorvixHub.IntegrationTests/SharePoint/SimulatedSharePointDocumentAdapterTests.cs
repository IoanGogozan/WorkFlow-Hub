using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NorvixHub.Application.SharePoint;
using NorvixHub.Application.Integrations;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Integrations;
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
        var request = await CreateRequestAsync(
            scope, tenantId, Guid.NewGuid(), "service-request.pdf", "CASE-2026-0014");

        var first = await adapter.SynchronizeAsync(request, TestContext.Current.CancellationToken);
        var second = await adapter.SynchronizeAsync(request, TestContext.Current.CancellationToken);

        first.Succeeded.Should().BeTrue();
        first.AlreadySynchronized.Should().BeFalse();
        second.Succeeded.Should().BeTrue();
        second.AlreadySynchronized.Should().BeTrue();
        (await dbContext.SimulatedSharePointDocumentItems.CountAsync(item => item.TenantId == tenantId, TestContext.Current.CancellationToken)).Should().Be(1);
        (await dbContext.SimulatedSharePointOperations.CountAsync(operation => operation.TenantId == tenantId, TestContext.Current.CancellationToken)).Should().Be(9);
        (await dbContext.SimulatedSharePointOperations.Where(operation => operation.TenantId == tenantId)
            .AllAsync(operation => operation.DurationMilliseconds > 0, TestContext.Current.CancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task New_version_updates_etag_and_stale_etag_is_rejected()
    {
        using var scope = _factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var tenantId = Guid.NewGuid();
        var first = await CreateRequestAsync(scope, tenantId, Guid.NewGuid(), "report.pdf", "CASE-2026-0015");
        var created = await adapter.SynchronizeAsync(first, TestContext.Current.CancellationToken);
        var secondVersion = await AddVersionAsync(scope, first);
        var updated = await adapter.SynchronizeAsync(secondVersion with { ExpectedETag = created.Item!.ETag }, TestContext.Current.CancellationToken);
        var thirdVersion = await AddVersionAsync(scope, first);
        var stale = await adapter.SynchronizeAsync(thirdVersion with { ExpectedETag = created.Item!.ETag }, TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task Listing_case_documents_is_tenant_scoped()
    {
        using var scope = _factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var caseId = Guid.NewGuid();
        var firstTenant = Guid.NewGuid();
        var secondTenant = Guid.NewGuid();
        await adapter.SynchronizeAsync(await CreateRequestAsync(scope, firstTenant, caseId, "a.pdf", "CASE-A"), TestContext.Current.CancellationToken);
        await adapter.SynchronizeAsync(await CreateRequestAsync(scope, secondTenant, caseId, "b.pdf", "CASE-B"), TestContext.Current.CancellationToken);

        var documents = await adapter.ListCaseDocumentsAsync(firstTenant, caseId, TestContext.Current.CancellationToken);

        documents.Should().ContainSingle();
        documents[0].Name.Should().Be("a.pdf");
        documents[0].ParentPath.Should().Contain("Incoming");
    }

    [Fact]
    public async Task Microsoft_graph_dashboard_sync_uses_real_tenant_operation_count()
    {
        using var scope = _factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var syncAdapter = scope.ServiceProvider.GetRequiredService<IIntegrationSyncAdapter>();
        var tenantWithOperations = Guid.NewGuid();
        var otherTenant = Guid.NewGuid();
        await adapter.TestSiteAccessAsync(tenantWithOperations, "site-demo-service", TestContext.Current.CancellationToken);
        await adapter.TestSiteAccessAsync(otherTenant, "site-demo-service", TestContext.Current.CancellationToken);
        await adapter.TestSiteAccessAsync(otherTenant, "site-hr-internal", TestContext.Current.CancellationToken);

        var result = await syncAdapter.SyncAsync(new IntegrationConnection { TenantId = tenantWithOperations, Provider = "microsoft-graph" }, TestContext.Current.CancellationToken);

        result.Succeeded.Should().BeTrue();
        result.ItemsProcessed.Should().Be(1);
    }

    [Fact]
    public async Task Opt_in_throttling_fails_once_with_429_and_retry_succeeds()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { ["SharePoint:SimulateThrottling"] = "true" })));
        using var scope = factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var request = await CreateRequestAsync(scope, Guid.NewGuid(), Guid.NewGuid(), "throttled.pdf", "CASE-429");

        var first = await adapter.SynchronizeAsync(request, TestContext.Current.CancellationToken);
        var retry = await adapter.SynchronizeAsync(request, TestContext.Current.CancellationToken);

        first.Succeeded.Should().BeFalse();
        first.StatusCode.Should().Be(429);
        first.ErrorCode.Should().Be("THROTTLED");
        retry.Succeeded.Should().BeTrue();
        retry.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Document_version_from_another_tenant_is_rejected_without_creating_an_item()
    {
        using var scope = _factory.Services.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ISharePointDocumentAdapterResolver>().GetCurrent();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var ownerTenant = Guid.NewGuid();
        var attackingTenant = Guid.NewGuid();
        var ownedRequest = await CreateRequestAsync(scope, ownerTenant, Guid.NewGuid(), "private.pdf", "CASE-PRIVATE");

        var result = await adapter.SynchronizeAsync(
            ownedRequest with { TenantId = attackingTenant },
            TestContext.Current.CancellationToken);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.ErrorCode.Should().Be("DOCUMENT_NOT_FOUND");
        (await dbContext.SimulatedSharePointDocumentItems.AnyAsync(
            item => item.TenantId == attackingTenant,
            TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    private static async Task<SharePointDocumentSyncRequest> CreateRequestAsync(
        IServiceScope scope,
        Guid tenantId,
        Guid caseId,
        string filename,
        string caseNumber)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var document = new DocumentRecord { TenantId = tenantId, Title = filename };
        var version = CreateVersion(tenantId, document.Id, filename, 1);
        document.SetCurrentVersion(version.Id, null, DateTimeOffset.UtcNow);
        dbContext.Documents.Add(document);
        dbContext.DocumentVersions.Add(version);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return new SharePointDocumentSyncRequest(
            tenantId, null, caseId, "Fjord Pumpeteknikk AS", caseNumber,
            document.Id, version.Id, filename, version.SizeBytes, "Report", "Approved");
    }

    private static async Task<SharePointDocumentSyncRequest> AddVersionAsync(
        IServiceScope scope,
        SharePointDocumentSyncRequest request)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var versionNumber = await dbContext.DocumentVersions.CountAsync(
            version => version.TenantId == request.TenantId && version.DocumentId == request.DocumentId,
            TestContext.Current.CancellationToken) + 1;
        var version = CreateVersion(request.TenantId, request.DocumentId, request.Filename, versionNumber);
        var document = await dbContext.Documents.SingleAsync(
            item => item.TenantId == request.TenantId && item.Id == request.DocumentId,
            TestContext.Current.CancellationToken);
        document.SetCurrentVersion(version.Id, null, DateTimeOffset.UtcNow);
        dbContext.DocumentVersions.Add(version);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return request with { DocumentVersionId = version.Id };
    }

    private static DocumentVersion CreateVersion(Guid tenantId, Guid documentId, string filename, int versionNumber) =>
        new()
        {
            TenantId = tenantId,
            DocumentId = documentId,
            VersionNumber = versionNumber,
            BlobContainer = "test",
            BlobName = Guid.NewGuid().ToString("N"),
            OriginalFilename = filename,
            ContentType = "application/pdf",
            SizeBytes = 100,
            Sha256Hash = new string('a', 64)
        };
}
