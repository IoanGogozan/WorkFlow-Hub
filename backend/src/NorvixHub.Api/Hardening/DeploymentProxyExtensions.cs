using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace NorvixHub.Api.Hardening;

public static class DeploymentProxyExtensions
{
    public static IServiceCollection AddDeploymentProxyReadiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection("Deployment")
            .Get<DeploymentProxyOptions>() ?? new DeploymentProxyOptions();

        services.Configure<DeploymentProxyOptions>(configuration.GetSection("Deployment"));
        services.AddHttpsRedirection(redirectionOptions =>
        {
            redirectionOptions.HttpsPort = options.HttpsPort;
        });
        services.Configure<ForwardedHeadersOptions>(forwardedOptions =>
        {
            forwardedOptions.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;
            forwardedOptions.ForwardLimit = Math.Max(1, options.ForwardLimit);

            forwardedOptions.KnownProxies.Clear();
            forwardedOptions.KnownIPNetworks.Clear();
            if (options.AllowUnknownProxies)
            {
                return;
            }

            foreach (var proxy in options.KnownProxies)
            {
                if (IPAddress.TryParse(proxy, out var address))
                {
                    forwardedOptions.KnownProxies.Add(address);
                }
            }

            foreach (var network in options.KnownNetworks)
            {
                if (System.Net.IPNetwork.TryParse(network, out var parsedNetwork))
                {
                    forwardedOptions.KnownIPNetworks.Add(parsedNetwork);
                }
            }

            forwardedOptions.KnownProxies.Add(IPAddress.Loopback);
            forwardedOptions.KnownProxies.Add(IPAddress.IPv6Loopback);
        });

        return services;
    }

    public static WebApplication UseDeploymentProxyReadiness(this WebApplication app)
    {
        var options = app.Configuration
            .GetSection("Deployment")
            .Get<DeploymentProxyOptions>() ?? new DeploymentProxyOptions();

        if (options.ForwardedHeadersEnabled)
        {
            app.UseForwardedHeaders();
        }

        if (options.EnforceHttps && !app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        return app;
    }
}
