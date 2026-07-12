using FluentAssertions;
using NorvixHub.Domain.LiveDemo;
using Xunit;

namespace NorvixHub.UnitTests.LiveDemo;

public sealed class LiveDemoRunTests
{
    [Fact]
    public void Queued_run_can_run_and_complete_with_duration()
    {
        var run = CreateRun();
        var startedAt = new DateTimeOffset(2026, 7, 11, 12, 0, 0, TimeSpan.Zero);

        run.MarkRunning("request-created", startedAt);
        run.SetCurrentStep("run-completed", startedAt.AddSeconds(2));
        run.MarkCompleted(startedAt.AddMilliseconds(8400));

        run.Status.Should().Be(LiveDemoRunStatus.Completed);
        run.TotalDurationMs.Should().Be(8400);
        run.CurrentStepKey.Should().Be("run-completed");
    }

    [Fact]
    public void Invalid_run_transitions_throw_predictable_exceptions()
    {
        var run = CreateRun();

        var complete = () => run.MarkCompleted(DateTimeOffset.UtcNow);
        complete.Should().Throw<InvalidOperationException>().WithMessage("Only a running live demo run can complete.");

        run.MarkRunning("request-created", DateTimeOffset.UtcNow);
        run.MarkCompleted(DateTimeOffset.UtcNow);
        var restart = () => run.MarkRunning("request-created", DateTimeOffset.UtcNow);
        restart.Should().Throw<InvalidOperationException>().WithMessage("Only a queued live demo run can start.");
    }

    [Fact]
    public void Failed_run_can_retry_only_up_to_limit()
    {
        var run = CreateRun();
        var now = DateTimeOffset.UtcNow;

        run.MarkRunning("erp-received", now);
        run.MarkFailed("ERP_RECEIVER_UNAVAILABLE", "ERP demo receiver svarer ikke.", now);
        run.QueueRetry(now);
        run.MarkRunning("erp-received", now);
        run.MarkFailed("ERP_RECEIVER_UNAVAILABLE", "ERP demo receiver svarer ikke.", now);
        run.QueueRetry(now);
        run.MarkRunning("erp-received", now);
        run.MarkFailed("ERP_RECEIVER_UNAVAILABLE", "ERP demo receiver svarer ikke.", now);

        var retry = () => run.QueueRetry(now);

        run.RetryCount.Should().Be(2);
        retry.Should().Throw<InvalidOperationException>().WithMessage("The live demo run retry limit has been reached.");
    }

    [Fact]
    public void Artifact_and_external_evidence_setters_preserve_references()
    {
        var run = CreateRun();
        var now = DateTimeOffset.UtcNow;
        var caseId = Guid.NewGuid();

        run.SetInternalArtifacts(Guid.NewGuid(), Guid.NewGuid(), caseId, Guid.NewGuid(), Guid.NewGuid(), now);
        run.SetBrregEvidence("live", now, now);
        run.SetSharePointEvidence("drive", "folder", "file", now);
        run.SetErpReceipt("ERP-RECEIPT-0142", now);

        run.CaseId.Should().Be(caseId);
        run.BrregMode.Should().Be("live");
        run.SharePointFileItemId.Should().Be("file");
        run.ErpReceiptId.Should().Be("ERP-RECEIPT-0142");
    }

    private static LiveDemoRun CreateRun() => new()
    {
        TenantId = Guid.NewGuid(),
        DemoSessionId = Guid.NewGuid(),
        ScenarioKey = "service-request",
        CorrelationId = "correlation-0142",
        OrganizationNumber = "999888777",
        CustomerReference = "CUST-0142",
        RequestTitle = "Fiktiv servicehenvendelse",
        RequestBody = "Fiktivt innhold"
    };
}
