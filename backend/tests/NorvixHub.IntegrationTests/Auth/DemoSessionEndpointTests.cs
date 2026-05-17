using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.Documents;
using NorvixHub.Api.Auth;
using NorvixHub.Contracts.Auth;
using NorvixHub.Contracts.Documents;
using NorvixHub.Contracts.Intake;
using NorvixHub.Domain.Demo;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Integrations;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Auth;

public sealed class DemoSessionEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public DemoSessionEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_demo_session_returns_token_and_seeds_isolated_tenant()
    {
        using var demoFactory = CreateDemoModeFactory();
        using var client = demoFactory.CreateClient();

        using var response = await client.PostAsync(
            "/api/demo-sessions",
            content: null,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateDemoSessionResponse>(
            TestContext.Current.CancellationToken);
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.DemoTenantId.Should().NotBeEmpty();
        body.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);

        using var scope = demoFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var session = await dbContext.DemoSessions.SingleAsync(
            candidate => candidate.Id == body.SessionId,
            TestContext.Current.CancellationToken);
        session.TenantId.Should().Be(body.DemoTenantId);
        session.TokenHash.Should().Be(DemoToken.Hash(body.Token));
        session.TokenHash.Should().NotBe(body.Token);
        session.Status.Should().Be(DemoSessionStatus.Active);

        var intakeCount = await dbContext.IntakeItems.CountAsync(
            intake => intake.TenantId == body.DemoTenantId,
            TestContext.Current.CancellationToken);
        intakeCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_demo_session_is_rate_limited()
    {
        using var demoFactory = CreateDemoModeFactoryWithRateLimits(
            demoSessionPermitLimit: 1,
            publicDeliveryPermitLimit: 100);
        using var client = demoFactory.CreateClient();

        using var first = await client.PostAsync(
            "/api/demo-sessions",
            content: null,
            TestContext.Current.CancellationToken);
        using var second = await client.PostAsync(
            "/api/demo-sessions",
            content: null,
            TestContext.Current.CancellationToken);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Public_delivery_endpoints_are_rate_limited()
    {
        using var demoFactory = CreateDemoModeFactoryWithRateLimits(
            demoSessionPermitLimit: 100,
            publicDeliveryPermitLimit: 1);
        using var client = demoFactory.CreateClient();

        using var first = await client.GetAsync(
            "/delivery/not-a-real-token",
            TestContext.Current.CancellationToken);
        using var second = await client.GetAsync(
            "/delivery/not-a-real-token",
            TestContext.Current.CancellationToken);

        first.StatusCode.Should().Be(HttpStatusCode.NotFound);
        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Demo_bearer_token_authenticates_current_user()
    {
        using var demoFactory = CreateDemoModeFactory();
        using var client = demoFactory.CreateClient();
        var session = await CreateDemoSessionAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(
            TestContext.Current.CancellationToken);
        body.Should().NotBeNull();
        body!.TenantId.Should().Be(session.DemoTenantId);
        body.Role.Should().Be("TenantOwner");
    }

    [Fact]
    public async Task Demo_session_cannot_access_another_demo_sessions_tenant_data()
    {
        using var demoFactory = CreateDemoModeFactory();
        using var client = demoFactory.CreateClient();
        var first = await CreateDemoSessionAsync(client);
        var second = await CreateDemoSessionAsync(client);

        using var createForSecond = new HttpRequestMessage(HttpMethod.Post, "/api/intakes")
        {
            Content = JsonContent.Create(new CreateIntakeRequest(
                "Manual",
                $"Second session intake {Guid.NewGuid():N}",
                "This intake belongs to the second demo session.",
                null,
                null,
                "Isolation",
                "Normal"))
        };
        createForSecond.Headers.Authorization = new AuthenticationHeaderValue("Bearer", second.Token);
        using var createResponse = await client.SendAsync(
            createForSecond,
            TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var secondIntake = await createResponse.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken);

        using var getWithFirst = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/intakes/{secondIntake!.Id}");
        getWithFirst.Headers.Authorization = new AuthenticationHeaderValue("Bearer", first.Token);
        using var getResponse = await client.SendAsync(
            getWithFirst,
            TestContext.Current.CancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Local_dev_headers_are_rejected_outside_development()
    {
        using var demoFactory = CreateDemoModeFactory();
        using var client = demoFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        DevAuthHeaders.AddDemoAdmin(request);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Public_demo_upload_is_rejected()
    {
        using var demoFactory = CreateDemoModeFactory();
        using var client = demoFactory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        using var request = CreateMultipartRequest(HttpMethod.Post, "/api/documents", "demo.pdf", "application/pdf");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Public_demo_can_create_and_download_sample_document()
    {
        using var demoFactory = CreateDemoModeFactory();
        using var client = demoFactory.CreateClient();
        var session = await CreateDemoSessionAsync(client);
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/documents/sample");
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);

        using var createResponse = await client.SendAsync(
            createRequest,
            TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var document = await createResponse.Content.ReadFromJsonAsync<DocumentResponse>(
            TestContext.Current.CancellationToken);
        document.Should().NotBeNull();
        document!.TenantId.Should().Be(session.DemoTenantId);
        document.Title.Should().Be("Demo inspection report");
        document.CurrentVersionId.Should().NotBeNull();

        using var downloadRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/documents/{document.Id}/download");
        downloadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        using var downloadResponse = await client.SendAsync(
            downloadRequest,
            TestContext.Current.CancellationToken);

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        downloadResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var content = await downloadResponse.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        content.Should().StartWith("%PDF"u8.ToArray());
    }

    [Fact]
    public async Task Expired_demo_session_token_is_rejected()
    {
        using var demoFactory = CreateDemoModeFactory();
        var token = await SeedExpiredDemoSessionAsync(demoFactory);
        using var client = demoFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cleanup_removes_expired_demo_workspace_and_keeps_active_workspace()
    {
        var expired = await SeedDemoWorkspaceAsync(_factory, isExpired: true);
        var active = await SeedDemoWorkspaceAsync(_factory, isExpired: false);

        using (var cleanupScope = _factory.Services.CreateScope())
        {
            var cleanupService = cleanupScope.ServiceProvider.GetRequiredService<DemoSessionCleanupService>();
            var result = await cleanupService.CleanupExpiredAsync(TestContext.Current.CancellationToken);
            result.SessionsDeleted.Should().BeGreaterThanOrEqualTo(1);
        }

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        (await dbContext.DemoSessions.AnyAsync(
            session => session.Id == expired.SessionId,
            TestContext.Current.CancellationToken)).Should().BeFalse();
        (await dbContext.Tenants.AnyAsync(
            tenant => tenant.Id == expired.TenantId,
            TestContext.Current.CancellationToken)).Should().BeFalse();
        (await dbContext.Users.AnyAsync(
            user => user.Id == expired.UserId,
            TestContext.Current.CancellationToken)).Should().BeFalse();
        (await dbContext.IntakeItems.AnyAsync(
            intake => intake.TenantId == expired.TenantId,
            TestContext.Current.CancellationToken)).Should().BeFalse();
        (await dbContext.IntegrationConnections.AnyAsync(
            connection => connection.TenantId == expired.TenantId,
            TestContext.Current.CancellationToken)).Should().BeFalse();

        (await dbContext.DemoSessions.AnyAsync(
            session => session.Id == active.SessionId,
            TestContext.Current.CancellationToken)).Should().BeTrue();
        (await dbContext.Tenants.AnyAsync(
            tenant => tenant.Id == active.TenantId,
            TestContext.Current.CancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task Cleanup_removes_local_files_for_expired_demo_workspace()
    {
        var expired = await SeedDemoWorkspaceAsync(_factory, isExpired: true, includeStoredFile: true);
        expired.StoredFile.Should().NotBeNull();

        using (var beforeScope = _factory.Services.CreateScope())
        {
            var fileStorage = beforeScope.ServiceProvider.GetRequiredService<IFileStorage>();
            var before = await fileStorage.OpenReadAsync(
                expired.StoredFile!.Container,
                expired.StoredFile.BlobName,
                TestContext.Current.CancellationToken);
            before.Should().NotBeNull();
            before!.Content.Dispose();
        }

        using (var cleanupScope = _factory.Services.CreateScope())
        {
            var cleanupService = cleanupScope.ServiceProvider.GetRequiredService<DemoSessionCleanupService>();
            var result = await cleanupService.CleanupExpiredAsync(TestContext.Current.CancellationToken);
            result.FilesDeleted.Should().BeGreaterThanOrEqualTo(1);
            result.FileDeleteFailures.Should().Be(0);
        }

        using var afterScope = _factory.Services.CreateScope();
        var afterStorage = afterScope.ServiceProvider.GetRequiredService<IFileStorage>();
        var after = await afterStorage.OpenReadAsync(
            expired.StoredFile!.Container,
            expired.StoredFile.BlobName,
            TestContext.Current.CancellationToken);
        after.Should().BeNull();
    }

    [Fact]
    public async Task Cleanup_missing_local_file_does_not_fail_whole_cleanup()
    {
        var expired = await SeedDemoWorkspaceAsync(_factory, isExpired: true, includeStoredFile: true);
        expired.StoredFile.Should().NotBeNull();

        using (var storageScope = _factory.Services.CreateScope())
        {
            var fileStorage = storageScope.ServiceProvider.GetRequiredService<IFileStorage>();
            await fileStorage.DeleteAsync(
                expired.StoredFile!.Container,
                expired.StoredFile.BlobName,
                TestContext.Current.CancellationToken);
        }

        using var cleanupScope = _factory.Services.CreateScope();
        var cleanupService = cleanupScope.ServiceProvider.GetRequiredService<DemoSessionCleanupService>();
        var result = await cleanupService.CleanupExpiredAsync(TestContext.Current.CancellationToken);
        result.SessionsDeleted.Should().BeGreaterThanOrEqualTo(1);
        result.FileDeleteFailures.Should().Be(0);
    }

    [Fact]
    public async Task Cleanup_does_not_delete_files_for_active_demo_workspace()
    {
        var active = await SeedDemoWorkspaceAsync(_factory, isExpired: false, includeStoredFile: true);
        active.StoredFile.Should().NotBeNull();

        using (var cleanupScope = _factory.Services.CreateScope())
        {
            var cleanupService = cleanupScope.ServiceProvider.GetRequiredService<DemoSessionCleanupService>();
            await cleanupService.CleanupExpiredAsync(TestContext.Current.CancellationToken);
        }

        using var storageScope = _factory.Services.CreateScope();
        var fileStorage = storageScope.ServiceProvider.GetRequiredService<IFileStorage>();
        var stored = await fileStorage.OpenReadAsync(
            active.StoredFile!.Container,
            active.StoredFile.BlobName,
            TestContext.Current.CancellationToken);
        stored.Should().NotBeNull();
        stored!.Content.Dispose();
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateDemoModeFactory()
    {
        return _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Demo"));
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateDemoModeFactoryWithRateLimits(
        int demoSessionPermitLimit,
        int publicDeliveryPermitLimit)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:DemoSessionCreation:PermitLimit"] = demoSessionPermitLimit.ToString(),
                    ["RateLimiting:DemoSessionCreation:WindowSeconds"] = "60",
                    ["RateLimiting:PublicDelivery:PermitLimit"] = publicDeliveryPermitLimit.ToString(),
                    ["RateLimiting:PublicDelivery:WindowSeconds"] = "60"
                });
            });
        });
    }

    private static async Task<CreateDemoSessionResponse> CreateDemoSessionAsync(HttpClient client)
    {
        using var response = await client.PostAsync(
            "/api/demo-sessions",
            content: null,
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateDemoSessionResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static HttpRequestMessage CreateMultipartRequest(
        HttpMethod method,
        string url,
        string filename,
        string contentType)
    {
        var fileContent = new ByteArrayContent("fake document content"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var multipart = new MultipartFormDataContent
        {
            { fileContent, "file", filename },
            { new StringContent("Uploaded document"), "title" }
        };

        return new HttpRequestMessage(method, url) { Content = multipart };
    }

    private static async Task<string> SeedExpiredDemoSessionAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory)
    {
        var now = DateTimeOffset.UtcNow;
        var token = DemoToken.Create();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        dbContext.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Expired Demo Tenant",
            Slug = $"expired-demo-{tenantId:N}"[..30],
            OrganizationNumber = "777777777"
        });
        dbContext.Users.Add(new UserProfile
        {
            Id = userId,
            DisplayName = "Expired Demo User",
            Email = $"expired.{userId:N}@workflow-demo.example"
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
            ExpiresAt = now.AddDays(-1)
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return token;
    }

    private static async Task<SeededDemoWorkspace> SeedDemoWorkspaceAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        bool isExpired,
        bool includeStoredFile = false)
    {
        var now = DateTimeOffset.UtcNow;
        var token = DemoToken.Create();
        var sessionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        StoredFileReference? storedFileReference = null;

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        dbContext.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = isExpired ? "Expired Cleanup Tenant" : "Active Cleanup Tenant",
            Slug = $"cleanup-{(isExpired ? "expired" : "active")}-{tenantId:N}"[..45],
            OrganizationNumber = isExpired ? "666666666" : "555555555"
        });
        dbContext.Users.Add(new UserProfile
        {
            Id = userId,
            DisplayName = isExpired ? "Expired Cleanup User" : "Active Cleanup User",
            Email = $"cleanup.{userId:N}@workflow-demo.example"
        });
        dbContext.TenantMemberships.Add(new TenantMembership
        {
            TenantId = tenantId,
            UserId = userId,
            Role = TenantRole.TenantOwner
        });
        dbContext.DemoSessions.Add(new DemoSession
        {
            Id = sessionId,
            TenantId = tenantId,
            UserId = userId,
            TokenHash = DemoToken.Hash(token),
            CreatedAt = isExpired ? now.AddDays(-2) : now,
            ExpiresAt = isExpired ? now.AddHours(-1) : now.AddHours(1)
        });
        dbContext.IntakeItems.Add(new IntakeItem
        {
            TenantId = tenantId,
            CreatedBy = userId,
            Source = IntakeSource.Manual,
            Subject = "Cleanup test intake",
            Body = "This intake should follow the demo tenant lifecycle."
        });
        dbContext.IntegrationConnections.Add(new IntegrationConnection
        {
            TenantId = tenantId,
            CreatedBy = userId,
            Provider = "cleanup-test",
            DisplayName = "Cleanup Test Integration"
        });
        if (includeStoredFile)
        {
            var fileBytes = System.Text.Encoding.UTF8.GetBytes($"Demo cleanup file {Guid.NewGuid():N}");
            await using var stream = new MemoryStream(fileBytes);
            var storedFile = await fileStorage.SaveAsync(
                stream,
                "cleanup-demo.pdf",
                "application/pdf",
                TestContext.Current.CancellationToken);
            var document = new DocumentRecord
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Title = "Cleanup demo document"
            };
            var version = new DocumentVersion
            {
                TenantId = tenantId,
                CreatedBy = userId,
                DocumentId = document.Id,
                VersionNumber = 1,
                BlobContainer = storedFile.Container,
                BlobName = storedFile.BlobName,
                OriginalFilename = "cleanup-demo.pdf",
                ContentType = "application/pdf",
                SizeBytes = storedFile.SizeBytes,
                Sha256Hash = storedFile.Sha256Hash,
                UploadedByUserId = userId
            };
            document.SetCurrentVersion(version.Id, userId, now);
            dbContext.Documents.Add(document);
            dbContext.DocumentVersions.Add(version);
            storedFileReference = new StoredFileReference(storedFile.Container, storedFile.BlobName);
        }
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return new SeededDemoWorkspace(sessionId, tenantId, userId, storedFileReference);
    }

    private sealed record SeededDemoWorkspace(
        Guid SessionId,
        Guid TenantId,
        Guid UserId,
        StoredFileReference? StoredFile);

    private sealed record StoredFileReference(string Container, string BlobName);
}
