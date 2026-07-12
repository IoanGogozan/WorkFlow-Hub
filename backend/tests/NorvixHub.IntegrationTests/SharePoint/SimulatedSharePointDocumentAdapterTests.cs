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
}
