using NorvixHub.Domain.Intake;

namespace NorvixHub.Application.AI;

public interface IAiReviewProvider
{
    string Provider { get; }
    string Model { get; }
    string PromptVersion { get; }

    AiIntakeSuggestion AnalyzeIntake(IntakeItem intake);
}

