using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.LiveDemo;

public sealed class LiveDemoRunStep : TenantScopedEntity
{
    public Guid RunId { get; init; }
    public required string Key { get; init; }
    public int Sequence { get; init; }
    public required string PublicStage { get; init; }
    public required string Provider { get; init; }
    public LiveDemoRunStepStatus Status { get; private set; } = LiveDemoRunStepStatus.Pending;
    public required string EvidenceMode { get; init; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string? PublicSummary { get; private set; }
    public string? PublicEvidenceReference { get; private set; }
    public string? PublicErrorCode { get; private set; }
    public string? PublicErrorMessage { get; private set; }

    public void MarkRunning(DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStepStatus.Pending, "Only a pending live demo step can start.");
        Status = LiveDemoRunStepStatus.Running;
        AttemptCount++;
        StartedAt = now;
        MarkUpdated(null, now);
    }

    public void MarkCompleted(string publicSummary, string? publicEvidenceReference, DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStepStatus.Running, "Only a running live demo step can complete.");
        Status = LiveDemoRunStepStatus.Completed;
        CompletedAt = now;
        DurationMs = CalculateDuration(now);
        PublicSummary = publicSummary;
        PublicEvidenceReference = publicEvidenceReference;
        PublicErrorCode = null;
        PublicErrorMessage = null;
        MarkUpdated(null, now);
    }

    public void MarkSkipped(string publicSummary, DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStepStatus.Pending, "Only a pending live demo step can be skipped.");
        Status = LiveDemoRunStepStatus.Skipped;
        CompletedAt = now;
        DurationMs = 0;
        PublicSummary = publicSummary;
        PublicEvidenceReference = null;
        PublicErrorCode = null;
        PublicErrorMessage = null;
        MarkUpdated(null, now);
    }

    public void MarkFailed(string publicErrorCode, string publicErrorMessage, DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStepStatus.Running, "Only a running live demo step can fail.");
        Status = LiveDemoRunStepStatus.Failed;
        CompletedAt = now;
        DurationMs = CalculateDuration(now);
        PublicErrorCode = publicErrorCode;
        PublicErrorMessage = publicErrorMessage;
        MarkUpdated(null, now);
    }

    public void ResetForRetry(DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStepStatus.Failed, "Only a failed live demo step can be reset for retry.");
        Status = LiveDemoRunStepStatus.Pending;
        StartedAt = null;
        CompletedAt = null;
        DurationMs = null;
        PublicErrorCode = null;
        PublicErrorMessage = null;
        MarkUpdated(null, now);
    }

    private long CalculateDuration(DateTimeOffset now) =>
        StartedAt is { } startedAt ? Math.Max(0, (long)(now - startedAt).TotalMilliseconds) : 0;

    private void EnsureStatus(LiveDemoRunStepStatus expected, string message)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(message);
        }
    }
}
