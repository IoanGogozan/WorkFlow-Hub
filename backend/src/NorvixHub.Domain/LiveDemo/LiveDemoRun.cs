using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.LiveDemo;

public sealed class LiveDemoRun : TenantScopedEntity
{
    public const int DefaultMaxRetries = 2;

    public Guid DemoSessionId { get; init; }
    public required string ScenarioKey { get; init; }
    public required string CorrelationId { get; init; }
    public LiveDemoRunStatus Status { get; private set; } = LiveDemoRunStatus.Queued;
    public string? CurrentStepKey { get; private set; }
    public required string OrganizationNumber { get; init; }
    public required string CustomerReference { get; init; }
    public required string RequestTitle { get; init; }
    public required string RequestBody { get; init; }
    public bool SimulateErpFailureOnce { get; init; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }
    public long? TotalDurationMs { get; private set; }
    public string? PublicErrorCode { get; private set; }
    public string? PublicErrorMessage { get; private set; }
    public Guid? IntakeItemId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? CaseId { get; private set; }
    public Guid? DocumentId { get; private set; }
    public Guid? DeliveryPackageId { get; private set; }
    public string? BrregMode { get; private set; }
    public DateTimeOffset? BrregSourceUpdatedAt { get; private set; }
    public string? SharePointDriveId { get; private set; }
    public string? SharePointFolderItemId { get; private set; }
    public string? SharePointFileItemId { get; private set; }
    public string? ErpReceiptId { get; private set; }

    public void MarkRunning(string stepKey, DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStatus.Queued, "Only a queued live demo run can start.");
        Status = LiveDemoRunStatus.Running;
        CurrentStepKey = stepKey;
        StartedAt ??= now;
        FailedAt = null;
        PublicErrorCode = null;
        PublicErrorMessage = null;
        MarkUpdated(null, now);
    }

    public void SetCurrentStep(string stepKey, DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStatus.Running, "Only a running live demo run can change step.");
        CurrentStepKey = stepKey;
        MarkUpdated(null, now);
    }

    public void MarkCompleted(DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStatus.Running, "Only a running live demo run can complete.");
        Status = LiveDemoRunStatus.Completed;
        CurrentStepKey = "run-completed";
        CompletedAt = now;
        FailedAt = null;
        TotalDurationMs = CalculateDuration(now);
        MarkUpdated(null, now);
    }

    public void MarkFailed(string publicErrorCode, string publicErrorMessage, DateTimeOffset now)
    {
        EnsureStatus(LiveDemoRunStatus.Running, "Only a running live demo run can fail.");
        Status = LiveDemoRunStatus.Failed;
        FailedAt = now;
        TotalDurationMs = CalculateDuration(now);
        PublicErrorCode = publicErrorCode;
        PublicErrorMessage = publicErrorMessage;
        MarkUpdated(null, now);
    }

    public void QueueRetry(DateTimeOffset now, int maxRetries = DefaultMaxRetries)
    {
        EnsureStatus(LiveDemoRunStatus.Failed, "Only a failed live demo run can be retried.");
        if (maxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries));
        }

        if (RetryCount >= maxRetries)
        {
            throw new InvalidOperationException("The live demo run retry limit has been reached.");
        }

        RetryCount++;
        Status = LiveDemoRunStatus.Queued;
        CurrentStepKey = null;
        FailedAt = null;
        CompletedAt = null;
        TotalDurationMs = null;
        PublicErrorCode = null;
        PublicErrorMessage = null;
        MarkUpdated(null, now);
    }

    public void SetInternalArtifacts(
        Guid? intakeItemId,
        Guid? customerId,
        Guid? caseId,
        Guid? documentId,
        Guid? deliveryPackageId,
        DateTimeOffset now)
    {
        IntakeItemId = intakeItemId ?? IntakeItemId;
        CustomerId = customerId ?? CustomerId;
        CaseId = caseId ?? CaseId;
        DocumentId = documentId ?? DocumentId;
        DeliveryPackageId = deliveryPackageId ?? DeliveryPackageId;
        MarkUpdated(null, now);
    }

    public void SetBrregEvidence(string mode, DateTimeOffset? sourceUpdatedAt, DateTimeOffset now)
    {
        BrregMode = mode;
        BrregSourceUpdatedAt = sourceUpdatedAt;
        MarkUpdated(null, now);
    }

    public void SetSharePointEvidence(
        string driveId,
        string folderItemId,
        string fileItemId,
        DateTimeOffset now)
    {
        SharePointDriveId = driveId;
        SharePointFolderItemId = folderItemId;
        SharePointFileItemId = fileItemId;
        MarkUpdated(null, now);
    }

    public void SetErpReceipt(string receiptId, DateTimeOffset now)
    {
        ErpReceiptId = receiptId;
        MarkUpdated(null, now);
    }

    private long CalculateDuration(DateTimeOffset now) =>
        StartedAt is { } startedAt ? Math.Max(0, (long)(now - startedAt).TotalMilliseconds) : 0;

    private void EnsureStatus(LiveDemoRunStatus expected, string message)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(message);
        }
    }
}
