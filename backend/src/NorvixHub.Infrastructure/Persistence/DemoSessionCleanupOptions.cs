namespace NorvixHub.Infrastructure.Persistence;

public sealed class DemoSessionCleanupOptions
{
    public bool Enabled { get; init; } = true;
    public int IntervalMinutes { get; init; } = 60;
    public int RetentionGraceMinutes { get; init; }
    public int BatchSize { get; init; } = 50;
}
