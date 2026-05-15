using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Intake;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class IntakeEndpoints
{
    public static IEndpointRouteBuilder MapIntakeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/intakes");

        group.MapGet("/", ListIntakes).WithName("ListIntakes");
        group.MapPost("/", CreateIntake).WithName("CreateIntake");
        group.MapGet("/{id:guid}", GetIntake).WithName("GetIntake");

        return app;
    }

    private static async Task<IResult> ListIntakes(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var intakes = await dbContext.IntakeItems
            .Where(intake => intake.TenantId == tenantId)
            .OrderByDescending(intake => intake.CreatedAt)
            .Select(intake => new IntakeListItemResponse(
                intake.Id,
                intake.Source.ToString(),
                intake.Status.ToString(),
                intake.Subject,
                intake.CustomerName,
                intake.Category,
                intake.Urgency,
                intake.ReceivedAt,
                intake.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(intakes);
    }

    private static async Task<IResult> CreateIntake(
        CreateIntakeRequest request,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanCreateIntake(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!TryValidate(request, out var source, out var error))
        {
            return Results.BadRequest(new { error });
        }

        var now = DateTimeOffset.UtcNow;
        var intake = new IntakeItem
        {
            TenantId = tenantContext.TenantId!.Value,
            CreatedBy = tenantContext.UserId,
            Source = source,
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim(),
            CustomerName = NormalizeOptional(request.CustomerName),
            OrganizationNumber = NormalizeOptional(request.OrganizationNumber),
            Category = NormalizeOptional(request.Category),
            Urgency = NormalizeOptional(request.Urgency),
            ReceivedAt = now
        };

        dbContext.IntakeItems.Add(intake);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, intake, tenantContext, httpContext, cancellationToken);

        return Results.Created($"/api/intakes/{intake.Id}", ToResponse(intake));
    }

    private static async Task<IResult> GetIntake(
        Guid id,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var intake = await dbContext.IntakeItems
            .Where(candidate => candidate.TenantId == tenantId && candidate.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        return intake is null ? Results.NotFound() : Results.Ok(ToResponse(intake));
    }

    private static bool CanCreateIntake(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or
            TenantRole.Admin or
            TenantRole.OperationsUser;
    }

    private static bool TryValidate(
        CreateIntakeRequest request,
        out IntakeSource source,
        out string error)
    {
        source = IntakeSource.Manual;
        error = string.Empty;

        if (!Enum.TryParse(request.Source, ignoreCase: true, out source))
        {
            error = "Invalid intake source.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Subject) || request.Subject.Length > 240)
        {
            error = "Subject is required and must be 240 characters or fewer.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Length > 8000)
        {
            error = "Body is required and must be 8000 characters or fewer.";
            return false;
        }

        return true;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static IntakeItemResponse ToResponse(IntakeItem intake)
    {
        return new IntakeItemResponse(
            intake.Id,
            intake.TenantId,
            intake.Source.ToString(),
            intake.Status.ToString(),
            intake.Subject,
            intake.Body,
            intake.CustomerName,
            intake.OrganizationNumber,
            intake.Category,
            intake.Urgency,
            intake.ReceivedAt,
            intake.CreatedAt);
    }

    private static Task WriteAuditAsync(
        IAuditEventWriter auditEventWriter,
        IntakeItem intake,
        ITenantContext tenantContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var request = new AuditEventRequest(
            intake.TenantId,
            tenantContext.UserId,
            "User",
            "IntakeItem",
            intake.Id.ToString(),
            "IntakeCreated",
            null,
            $$"""{"status":"{{intake.Status}}","source":"{{intake.Source}}"}""",
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            httpContext.TraceIdentifier);

        return auditEventWriter.WriteAsync(request, cancellationToken);
    }
}
