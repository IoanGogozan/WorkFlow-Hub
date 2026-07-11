using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NorvixHub.Api.RateLimiting;
using Xunit;

namespace NorvixHub.IntegrationTests.RateLimiting;

public sealed class LiveDemoRateLimitingTests
{
    [Fact]
    public void Live_demo_run_creation_policy_binds_safe_defaults_and_overrides()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:LiveDemoRunCreation:PermitLimit"] = "2",
                ["RateLimiting:LiveDemoRunCreation:WindowSeconds"] = "300"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddPublicDemoRateLimiting(configuration);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PublicDemoRateLimitOptions>>().Value;

        PublicDemoRateLimiting.LiveDemoRunCreationPolicy.Should().Be("live-demo-run-creation");
        options.LiveDemoRunCreation.PermitLimit.Should().Be(2);
        options.LiveDemoRunCreation.WindowSeconds.Should().Be(300);
    }
}
