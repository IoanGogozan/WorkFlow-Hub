using FluentAssertions;
using Microsoft.Extensions.Options;
using NorvixHub.Application.SharePoint;
using NorvixHub.Infrastructure.SharePoint;
using Xunit;

namespace NorvixHub.UnitTests.SharePoint;

public sealed class SharePointDocumentAdapterResolverTests
{
    [Fact]
    public void Default_mode_resolves_the_local_simulator()
    {
        var adapter = CreateResolver(new SharePointOptions()).GetCurrent();

        var status = adapter.GetStatus();

        adapter.Mode.Should().Be("Simulated");
        status.IsSimulated.Should().BeTrue();
        status.IsConfigured.Should().BeTrue();
        status.SiteId.Should().Be("site-demo-service");
        status.PublicMessage.Should().Contain("No Microsoft 365 tenant is connected");
    }

    [Fact]
    public void Microsoft_graph_mode_returns_a_safe_not_configured_status_when_values_are_missing()
    {
        var adapter = CreateResolver(new SharePointOptions { Mode = "MicrosoftGraph" }).GetCurrent();

        var status = adapter.GetStatus();

        adapter.Mode.Should().Be("MicrosoftGraph");
        status.IsSimulated.Should().BeFalse();
        status.IsConfigured.Should().BeFalse();
        status.PublicMessage.Should().Be("Microsoft Graph provider is not configured.");
        status.PublicMessage.Should().NotContain("secret");
    }

    [Fact]
    public void Unsupported_mode_fails_without_echoing_configuration_value()
    {
        var action = () => CreateResolver(new SharePointOptions { Mode = "UntrustedProvider" }).GetCurrent();

        action.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Be("SharePoint provider mode is not supported.");
    }

    private static ISharePointDocumentAdapterResolver CreateResolver(SharePointOptions options)
    {
        var optionWrapper = Options.Create(options);
        return new SharePointDocumentAdapterResolver(
            optionWrapper,
            new SimulatedSharePointDocumentAdapter(optionWrapper),
            new MicrosoftGraphSharePointDocumentAdapter(optionWrapper));
    }
}
