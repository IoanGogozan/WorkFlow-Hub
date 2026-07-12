using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace NorvixHub.Api.RateLimiting;

public static class PublicDemoRateLimiting
{
    public const string DemoSessionCreationPolicy = "demo-session-creation";
    public const string PublicDeliveryPolicy = "public-delivery";
    public const string LiveDemoRunCreationPolicy = "live-demo-run-creation";

    public static IServiceCollection AddPublicDemoRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PublicDemoRateLimitOptions>(
            configuration.GetSection("RateLimiting"));

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rateLimiterOptions.AddPolicy(
                DemoSessionCreationPolicy,
                context =>
                {
                    var options = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<PublicDemoRateLimitOptions>>()
                        .CurrentValue;
                    return CreateFixedWindowLimiter(context, options.DemoSessionCreation);
                });
            rateLimiterOptions.AddPolicy(
                PublicDeliveryPolicy,
                context =>
                {
                    var options = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<PublicDemoRateLimitOptions>>()
                        .CurrentValue;
                    return CreateFixedWindowLimiter(context, options.PublicDelivery);
                });
            rateLimiterOptions.AddPolicy(
                LiveDemoRunCreationPolicy,
                context =>
                {
                    var options = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<PublicDemoRateLimitOptions>>()
                        .CurrentValue;
                    return CreateFixedWindowLimiter(context, options.LiveDemoRunCreation);
                });
        });

        return services;
    }

    private static RateLimitPartition<string> CreateFixedWindowLimiter(
        HttpContext context,
        FixedWindowRateLimitOptions options)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = Math.Max(1, options.PermitLimit),
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(Math.Max(1, options.WindowSeconds))
            });
    }

    private static string GetClientPartitionKey(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
