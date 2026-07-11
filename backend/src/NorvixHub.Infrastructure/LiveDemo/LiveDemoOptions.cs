namespace NorvixHub.Infrastructure.LiveDemo;

public sealed class LiveDemoOptions
{
    public bool Enabled { get; set; }
    public string OrganizationNumber { get; set; } = string.Empty;
    public int MaxRunsPerSession { get; set; } = 3;
    public int MaxRetriesPerRun { get; set; } = 2;
    public int WorkerPollMilliseconds { get; set; } = 1000;
    public int RunRecoveryMinutes { get; set; } = 5;
    public bool BrregFallbackEnabled { get; set; } = true;
}
