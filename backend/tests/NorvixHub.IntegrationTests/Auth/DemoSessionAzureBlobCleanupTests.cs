using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Api.Auth;
using NorvixHub.Application.Documents;
using NorvixHub.Domain.Demo;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace NorvixHub.IntegrationTests.Auth;

public sealed class DemoSessionAzureBlobCleanupTests :
    IClassFixture<NorvixHubApiFactory>,
    IAsyncLifetime
{
    private const string AzuriteAccountName = "devstoreaccount1";
    private const string AzuriteAccountKey =
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/" +
        "K1SZFPTOtr/KBHBeksoGMGw==";

    private readonly NorvixHubApiFactory factory;
    private readonly IContainer azurite;

    public DemoSessionAzureBlobCleanupTests(NorvixHubApiFactory factory)
    {
        this.factory = factory;
        azurite = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .WithPortBinding(10000, true)
            .WithCommand("azurite-blob", "--blobHost", "0.0.0.0")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(10000))
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await azurite.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await azurite.DisposeAsync();
    }

    [Fact]
    public async Task Cleanup_removes_azure_blob_files_for_expired_demo_workspace()
    {
        var containerName = $"demo-cleanup-{Guid.NewGuid():N}";
        using var azureFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Storage:Provider"] = "AzureBlob",
                    ["Storage:AzureBlob:ConnectionString"] = CreateAzuriteConnectionString(),
                    ["Storage:AzureBlob:Container"] = containerName,
                    ["DemoSessionCleanup:RetentionGraceMinutes"] = "0"
                });
            });
        });

        var expired = await SeedExpiredWorkspaceWithStoredFileAsync(azureFactory);

        using (var beforeScope = azureFactory.Services.CreateScope())
        {
            var fileStorage = beforeScope.ServiceProvider.GetRequiredService<IFileStorage>();
            var before = await fileStorage.OpenReadAsync(
                expired.Container,
                expired.BlobName,
                TestContext.Current.CancellationToken);
            before.Should().NotBeNull();
            before!.Content.Dispose();
        }

        using (var cleanupScope = azureFactory.Services.CreateScope())
        {
            var cleanupService = cleanupScope.ServiceProvider.GetRequiredService<DemoSessionCleanupService>();
            var result = await cleanupService.CleanupExpiredAsync(TestContext.Current.CancellationToken);
            result.FilesDeleted.Should().BeGreaterThanOrEqualTo(1);
            result.FileDeleteFailures.Should().Be(0);
        }

        using var afterScope = azureFactory.Services.CreateScope();
        var afterStorage = afterScope.ServiceProvider.GetRequiredService<IFileStorage>();
        var after = await afterStorage.OpenReadAsync(
            expired.Container,
            expired.BlobName,
            TestContext.Current.CancellationToken);
        after.Should().BeNull();
    }

    private async Task<StoredFileReference> SeedExpiredWorkspaceWithStoredFileAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> azureFactory)
    {
        var now = DateTimeOffset.UtcNow;
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = DemoToken.Create();

        using var scope = azureFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

        dbContext.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Expired Azure Blob Cleanup Tenant",
            Slug = $"azure-cleanup-{tenantId:N}"[..45],
            OrganizationNumber = "444444444"
        });
        dbContext.Users.Add(new UserProfile
        {
            Id = userId,
            DisplayName = "Expired Azure Blob Cleanup User",
            Email = $"azure.cleanup.{userId:N}@workflow-demo.example"
        });
        dbContext.TenantMemberships.Add(new TenantMembership
        {
            TenantId = tenantId,
            UserId = userId,
            Role = TenantRole.TenantOwner
        });
        dbContext.DemoSessions.Add(new DemoSession
        {
            TenantId = tenantId,
            UserId = userId,
            TokenHash = DemoToken.Hash(token),
            CreatedAt = now.AddDays(-2),
            ExpiresAt = now.AddHours(-1)
        });

        var fileBytes = System.Text.Encoding.UTF8.GetBytes($"Azure cleanup file {Guid.NewGuid():N}");
        await using var stream = new MemoryStream(fileBytes);
        var storedFile = await fileStorage.SaveAsync(
            stream,
            "azure-cleanup-demo.pdf",
            "application/pdf",
            TestContext.Current.CancellationToken);

        var document = new DocumentRecord
        {
            TenantId = tenantId,
            CreatedBy = userId,
            Title = "Azure cleanup demo document"
        };
        var version = new DocumentVersion
        {
            TenantId = tenantId,
            CreatedBy = userId,
            DocumentId = document.Id,
            VersionNumber = 1,
            BlobContainer = storedFile.Container,
            BlobName = storedFile.BlobName,
            OriginalFilename = "azure-cleanup-demo.pdf",
            ContentType = "application/pdf",
            SizeBytes = storedFile.SizeBytes,
            Sha256Hash = storedFile.Sha256Hash,
            UploadedByUserId = userId
        };
        document.SetCurrentVersion(version.Id, userId, now);
        dbContext.Documents.Add(document);
        dbContext.DocumentVersions.Add(version);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return new StoredFileReference(storedFile.Container, storedFile.BlobName);
    }

    private string CreateAzuriteConnectionString()
    {
        var mappedPort = azurite.GetMappedPublicPort(10000);
        return "DefaultEndpointsProtocol=http;" +
            $"AccountName={AzuriteAccountName};" +
            $"AccountKey={AzuriteAccountKey};" +
            $"BlobEndpoint=http://127.0.0.1:{mappedPort}/{AzuriteAccountName};";
    }

    private sealed record StoredFileReference(string Container, string BlobName);
}
