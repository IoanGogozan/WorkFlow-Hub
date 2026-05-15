using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Reviews;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class ReviewTaskEndpoints
{
    public static IEndpointRouteBuilder MapReviewTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/review-tasks", ListReviewTasks).WithName("ListReviewTasks");
        return app;
    }

    private static async Task<IResult> ListReviewTasks(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var tasks = await dbContext.ReviewTasks
            .Where(task => task.TenantId == tenantId)
            .OrderByDescending(task => task.CreatedAt)
            .Select(task => new ReviewTaskResponse(
                task.Id,
                task.EntityType,
                task.EntityId,
                task.ReviewType,
                task.Status.ToString(),
                task.AiAnalysisRunId,
                task.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(tasks);
    }
}

