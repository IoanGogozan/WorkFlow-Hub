using FluentAssertions;
using NorvixHub.Domain.LiveDemo;
using Xunit;

namespace NorvixHub.UnitTests.LiveDemo;

public sealed class LiveDemoRunStepTests
{
    [Fact]
    public void Step_can_run_and_complete_once()
    {
        var step = CreateStep();
        var startedAt = new DateTimeOffset(2026, 7, 11, 12, 0, 0, TimeSpan.Zero);

        step.MarkRunning(startedAt);
        step.MarkCompleted("Sak opprettet.", "LIVE-2026-0142", startedAt.AddMilliseconds(3600));

        step.Status.Should().Be(LiveDemoRunStepStatus.Completed);
        step.AttemptCount.Should().Be(1);
        step.DurationMs.Should().Be(3600);
        var rerun = () => step.MarkRunning(startedAt);
        rerun.Should().Throw<InvalidOperationException>().WithMessage("Only a pending live demo step can start.");
    }

    [Fact]
    public void Failed_step_can_reset_and_run_again()
    {
        var step = CreateStep();
        var now = DateTimeOffset.UtcNow;

        step.MarkRunning(now);
        step.MarkFailed("ERP_RECEIVER_UNAVAILABLE", "ERP demo receiver svarer ikke.", now);
        step.ResetForRetry(now);
        step.MarkRunning(now);

        step.Status.Should().Be(LiveDemoRunStepStatus.Running);
        step.AttemptCount.Should().Be(2);
        step.PublicErrorCode.Should().BeNull();
    }

    [Fact]
    public void Invalid_step_transitions_throw_predictable_exceptions()
    {
        var step = CreateStep();

        var reset = () => step.ResetForRetry(DateTimeOffset.UtcNow);
        reset.Should().Throw<InvalidOperationException>().WithMessage("Only a failed live demo step can be reset for retry.");
        var complete = () => step.MarkCompleted("Done", null, DateTimeOffset.UtcNow);
        complete.Should().Throw<InvalidOperationException>().WithMessage("Only a running live demo step can complete.");
    }

    [Fact]
    public void Pending_step_can_be_skipped_as_a_terminal_step()
    {
        var step = CreateStep();
        var now = DateTimeOffset.UtcNow;

        step.MarkSkipped("Capability is disabled.", now);

        step.Status.Should().Be(LiveDemoRunStepStatus.Skipped);
        step.CompletedAt.Should().Be(now);
        step.DurationMs.Should().Be(0);
        step.AttemptCount.Should().Be(0);
        var start = () => step.MarkRunning(now);
        start.Should().Throw<InvalidOperationException>();
    }

    private static LiveDemoRunStep CreateStep() => new()
    {
        TenantId = Guid.NewGuid(),
        RunId = Guid.NewGuid(),
        Key = "case-created",
        Sequence = 3,
        PublicStage = "Opprettet",
        Provider = "Norvix WorkFlow Hub",
        EvidenceMode = "implemented"
    };
}
