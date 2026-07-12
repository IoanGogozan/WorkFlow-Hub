using FluentAssertions;
using Microsoft.Extensions.Options;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Application.Organizations;
using NorvixHub.Infrastructure.LiveDemo;
using Xunit;

namespace NorvixHub.UnitTests.LiveDemo;

public sealed class LiveDemoOrganizationResolverTests
{
    [Fact]
    public async Task Resolve_returns_live_public_data_on_success()
    {
        var resolver = CreateResolver(_ => Task.FromResult<BrregOrganization?>(new(
            "999888777", "Fiktiv Drift AS", "AS", "Kristiansand", null, false,
            DateTimeOffset.Parse("2026-07-12T10:00:00Z"), "raw external data")));

        var result = await resolver.ResolveAsync("999888777", TestContext.Current.CancellationToken);

        result.Mode.Should().Be("live");
        result.Organization.Name.Should().Be("Fiktiv Drift AS");
        result.Organization.SourceUpdatedAt.Should().Be(DateTimeOffset.Parse("2026-07-12T10:00:00Z"));
        result.SafeReason.Should().BeNull();
    }

    [Fact]
    public async Task Resolve_not_found_throws_public_safe_error()
    {
        var resolver = CreateResolver(_ => Task.FromResult<BrregOrganization?>(null));

        var action = () => resolver.ResolveAsync("999888777", TestContext.Current.CancellationToken);

        var error = await action.Should().ThrowAsync<LiveDemoOrganizationResolutionException>();
        error.Which.Code.Should().Be("BRREG_UNAVAILABLE");
        error.Which.PublicMessage.Should().NotContain("999888777");
    }

    [Fact]
    public async Task Resolve_timeout_uses_fallback_after_one_bounded_retry()
    {
        var calls = 0;
        var resolver = CreateResolver(_ =>
        {
            calls++;
            throw new TimeoutException("network timeout");
        });

        var result = await resolver.ResolveAsync("999888777", TestContext.Current.CancellationToken);

        calls.Should().Be(2);
        result.Mode.Should().Be("fallback");
        result.Organization.Name.Should().Be("Fiktiv Brreg demo snapshot AS");
    }

    [Fact]
    public async Task Resolve_transient_http_failure_returns_configured_fallback()
    {
        var resolver = CreateResolver(_ => throw new HttpRequestException("remote unavailable"));

        var result = await resolver.ResolveAsync("999888777", TestContext.Current.CancellationToken);

        result.Mode.Should().Be("fallback");
        result.SafeReason.Should().Be("Brreg svarte ikke innenfor den avgrensede demoforespørselen.");
        result.Organization.OrganizationForm.Should().Be("AS");
    }

    [Fact]
    public async Task Resolve_transient_failure_without_fallback_throws_public_safe_error()
    {
        var resolver = CreateResolver(
            _ => throw new HttpRequestException("remote unavailable"),
            fallbackEnabled: false);

        var action = () => resolver.ResolveAsync("999888777", TestContext.Current.CancellationToken);

        var error = await action.Should().ThrowAsync<LiveDemoOrganizationResolutionException>();
        error.Which.Code.Should().Be("BRREG_UNAVAILABLE");
    }

    [Fact]
    public async Task Resolve_does_not_expose_raw_external_exception_text()
    {
        var resolver = CreateResolver(_ => throw new InvalidOperationException("upstream token leaked: secret-value"));

        var action = () => resolver.ResolveAsync("999888777", TestContext.Current.CancellationToken);

        var error = await action.Should().ThrowAsync<LiveDemoOrganizationResolutionException>();
        error.Which.PublicMessage.Should().NotContain("secret-value");
        error.Which.Message.Should().NotContain("upstream token");
    }

    private static LiveDemoOrganizationResolver CreateResolver(
        Func<string, Task<BrregOrganization?>> getByOrganizationNumber,
        bool fallbackEnabled = true)
    {
        return new LiveDemoOrganizationResolver(
            new StubBrregClient(getByOrganizationNumber),
            Options.Create(new LiveDemoOptions { BrregFallbackEnabled = fallbackEnabled }));
    }

    private sealed class StubBrregClient(Func<string, Task<BrregOrganization?>> getByOrganizationNumber) : IBrregClient
    {
        public Task<IReadOnlyList<BrregOrganization>> SearchAsync(string query, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BrregOrganization>>([]);

        public Task<BrregOrganization?> GetByOrganizationNumberAsync(
            string organizationNumber,
            CancellationToken cancellationToken) => getByOrganizationNumber(organizationNumber);
    }
}
