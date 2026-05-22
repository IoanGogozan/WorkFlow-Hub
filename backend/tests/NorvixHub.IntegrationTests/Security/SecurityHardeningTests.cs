using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Intake;
using NorvixHub.Infrastructure.Persistence;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Security;

public sealed class SecurityHardeningTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public SecurityHardeningTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Responses_include_security_headers()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(
            "/health",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("no-referrer");
        response.Headers.GetValues("Permissions-Policy").Should()
            .Contain("camera=(), microphone=(), geolocation=()");
        response.Headers.GetValues("Content-Security-Policy").Should()
            .Contain("default-src 'none'; frame-ancestors 'none'");
    }

    [Fact]
    public async Task Responses_include_generated_correlation_id()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(
            "/health",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Correlation-ID").Single().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Request_correlation_id_is_returned_and_written_to_audit_events()
    {
        const string correlationId = "demo-correlation-123";
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/intakes")
        {
            Content = JsonContent.Create(new CreateIntakeRequest(
                "Manual",
                $"Correlation test {Guid.NewGuid():N}",
                "Customer asks for documentation follow-up.",
                "Sordal Eiendom AS",
                "999888777",
                "Documentation",
                "Normal"))
        };
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);
        DevAuthHeaders.AddDemoAdmin(request);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.GetValues("X-Correlation-ID").Single().Should().Be(correlationId);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
        var auditEvent = await dbContext.AuditEvents
            .Where(auditEvent =>
                auditEvent.TenantId == LocalDevTenantContext.DemoTenantId &&
                auditEvent.Action == "IntakeCreated" &&
                auditEvent.CorrelationId == correlationId)
            .SingleOrDefaultAsync(TestContext.Current.CancellationToken);

        auditEvent.Should().NotBeNull();
    }

    [Fact]
    public async Task Demo_environment_unhandled_exception_returns_clean_problem_without_stack_trace()
    {
        using var demoFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Test:EnableExceptionProbe"] = "true"
                });
            });
        });
        using var client = demoFactory.CreateClient();

        using var response = await client.GetAsync(
            "/__test/throw",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Headers.GetValues("X-Correlation-ID").Single().Should().NotBeNullOrWhiteSpace();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("An unexpected error occurred.");
        body.Should().NotContain("InvalidOperationException");
        body.Should().NotContain("Sensitive stack trace detail");
        body.Should().NotContain("SecurityHardeningTests");
    }

    [Fact]
    public async Task Forwarded_headers_update_scheme_host_and_https_state()
    {
        using var demoFactory = CreateProxyFactory(enforceHttps: false);
        using var client = demoFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/__test/request-info");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", "demo.norvix.no");
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.10");

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("\"scheme\":\"https\"");
        body.Should().Contain("\"host\":\"demo.norvix.no\"");
        body.Should().Contain("\"isHttps\":true");
        body.Should().Contain("\"remoteIpAddress\":\"203.0.113.10\"");
    }

    [Fact]
    public async Task Enforce_https_redirects_plain_http_requests()
    {
        using var demoFactory = CreateProxyFactory(enforceHttps: true);
        using var client = demoFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync(
            "/health",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.TemporaryRedirect);
        response.Headers.Location!.Scheme.Should().Be("https");
    }

    [Fact]
    public async Task Enforce_https_respects_forwarded_https_and_emits_hsts()
    {
        using var demoFactory = CreateProxyFactory(enforceHttps: true);
        using var client = demoFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", "demo.norvix.no");

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("Strict-Transport-Security").Should()
            .Contain(value => value.Contains("max-age", StringComparison.OrdinalIgnoreCase));
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateProxyFactory(
        bool enforceHttps)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Demo");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Test:EnableExceptionProbe"] = "true",
                    ["Deployment:ForwardedHeadersEnabled"] = "true",
                    ["Deployment:EnforceHttps"] = enforceHttps.ToString(),
                    ["Deployment:HttpsPort"] = "443",
                    ["Deployment:ForwardLimit"] = "1",
                    ["Deployment:AllowUnknownProxies"] = "false"
                });
            });
        });
    }
}
