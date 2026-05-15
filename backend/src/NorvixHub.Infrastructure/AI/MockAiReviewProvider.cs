using NorvixHub.Application.AI;
using NorvixHub.Domain.Intake;

namespace NorvixHub.Infrastructure.AI;

public sealed class MockAiReviewProvider : IAiReviewProvider
{
    public string Provider => "Mock";
    public string Model => "mock-intake-review-v1";
    public string PromptVersion => "intake-review-2026-05-15";

    public AiIntakeSuggestion AnalyzeIntake(IntakeItem intake)
    {
        var category = intake.Category ?? InferCategory(intake);
        var urgency = intake.Urgency ?? InferUrgency(intake);

        return new AiIntakeSuggestion(
            intake.CustomerName ?? "Sordal Eiendom AS",
            intake.OrganizationNumber ?? "999888777",
            category,
            urgency,
            CreateTasks(category),
            $"Request about {category.ToLowerInvariant()} should be reviewed by operations.",
            new[] { "Confirm customer contact person", "Attach relevant documentation" },
            0.82m);
    }

    private static string InferCategory(IntakeItem intake)
    {
        return intake.Body.Contains("document", StringComparison.OrdinalIgnoreCase)
            ? "Documentation"
            : "Operations";
    }

    private static string InferUrgency(IntakeItem intake)
    {
        return intake.Body.Contains("urgent", StringComparison.OrdinalIgnoreCase)
            ? "High"
            : "Normal";
    }

    private static IReadOnlyList<string> CreateTasks(string category)
    {
        return new[]
        {
            $"Review {category.ToLowerInvariant()} request",
            "Check missing information",
            "Prepare customer follow-up"
        };
    }
}

