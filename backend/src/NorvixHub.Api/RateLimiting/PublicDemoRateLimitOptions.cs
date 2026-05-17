namespace NorvixHub.Api.RateLimiting;

public sealed class PublicDemoRateLimitOptions
{
    public FixedWindowRateLimitOptions DemoSessionCreation { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60
    };

    public FixedWindowRateLimitOptions PublicDelivery { get; set; } = new()
    {
        PermitLimit = 120,
        WindowSeconds = 60
    };
}

public sealed class FixedWindowRateLimitOptions
{
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
}
