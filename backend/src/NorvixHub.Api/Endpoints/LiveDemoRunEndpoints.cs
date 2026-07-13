using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Tenancy;
using NorvixHub.Api.Auth;
using NorvixHub.Api.RateLimiting;
using NorvixHub.Contracts.LiveDemo;
using NorvixHub.Domain.Demo;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Infrastructure.LiveDemo;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class LiveDemoRunEndpoints
{
    private const string UnavailableError = "Live demo is not available.";

    public static IEndpointRouteBuilder MapLiveDemoRunEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/live-demo-runs", CreateLiveDemoRun)
            .RequireRateLimiting(PublicDemoRateLimiting.LiveDemoRunCreationPolicy)
            .WithName("CreateLiveDemoRun");
        app.MapGet("/api/live-demo-runs/{runId:guid}", GetLiveDemoRun)
            .WithName("GetLiveDemoRun");
        app.MapGet("/api/live-demo-capabilities", GetLiveDemoCapabilities)
            .WithName("GetLiveDemoCapabilities");
        app.MapPost("/api/live-demo-runs/{runId:guid}/retry", RetryLiveDemoRun)
            .WithName("RetryLiveDemoRun");

        return app;
    }

    private static async Task<IResult> CreateLiveDemoRun(
        CreateLiveDemoRunRequest request,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IOptions<LiveDemoOptions> liveDemoOptions,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId || tenantContext.UserId is not { } userId ||
            !TryGetBearerToken(httpContext, out var token))
        {
            return Results.Unauthorized();
        }

        var options = liveDemoOptions.Value;
        if (!options.Enabled || string.IsNullOrWhiteSpace(options.OrganizationNumber))
        {
            return Results.Json(new { error = UnavailableError }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var now = DateTimeOffset.UtcNow;
        var session = await dbContext.DemoSessions.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == DemoToken.Hash(token) &&
                candidate.TenantId == tenantId &&
                candidate.UserId == userId &&
                candidate.Status == DemoSessionStatus.Active &&
                candidate.ExpiresAt > now,
            cancellationToken);
        if (session is null)
        {
            return Results.Unauthorized();
        }

        var hasActiveRun = await dbContext.LiveDemoRuns.AnyAsync(
            candidate => candidate.TenantId == tenantId &&
                candidate.DemoSessionId == session.Id &&
                (candidate.Status == LiveDemoRunStatus.Queued || candidate.Status == LiveDemoRunStatus.Running),
            cancellationToken);
        if (hasActiveRun)
        {
            return Results.Conflict(new { error = "A live demo run is already in progress." });
        }

        var runCount = await dbContext.LiveDemoRuns.CountAsync(
            candidate => candidate.TenantId == tenantId && candidate.DemoSessionId == session.Id,
            cancellationToken);
        if (runCount >= Math.Max(1, options.MaxRunsPerSession))
        {
            return Results.Conflict(new { error = "The maximum number of live demo runs has been reached." });
        }

        var run = CreatePresetRun(tenantId, userId, session.Id, options.OrganizationNumber, request, now);
        dbContext.LiveDemoRuns.Add(run);
        dbContext.LiveDemoRunSteps.AddRange(CreatePendingSteps(tenantId, userId, run.Id));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Accepted(
            $"/api/live-demo-runs/{run.Id}",
            new CreateLiveDemoRunResponse(
                run.Id,
                run.Status.ToString(),
                $"/api/live-demo-runs/{run.Id}",
                run.CreatedAt));
    }

    private static LiveDemoRun CreatePresetRun(
        Guid tenantId,
        Guid userId,
        Guid demoSessionId,
        string organizationNumber,
        CreateLiveDemoRunRequest request,
        DateTimeOffset now)
    {
        var referenceSuffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return new LiveDemoRun
        {
            TenantId = tenantId,
            CreatedBy = userId,
            DemoSessionId = demoSessionId,
            ScenarioKey = "pump-station-service",
            CorrelationId = Guid.NewGuid().ToString("N"),
            OrganizationNumber = organizationNumber,
            CustomerReference = $"LIVE-{now:yyyy}-{referenceSuffix}",
            RequestTitle = "Fiktiv servicehenvendelse – pumpestasjon 14",
            RequestBody = "Fiktiv henvendelse for live-demo. Opprett sak og dokumentasjon for planlagt service på pumpestasjon 14.",
            SimulateErpFailureOnce = request.SimulateErpFailureOnce
        };
    }

    private static async Task<IResult> GetLiveDemoRun(
        Guid runId,
        ITenantContext tenantContext,
        IOptions<LiveDemoOptions> liveDemoOptions,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var run = await dbContext.LiveDemoRuns
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == runId && candidate.TenantId == tenantId, cancellationToken);
        if (run is null)
        {
            return Results.NotFound(new { error = "Live demo run was not found." });
        }

        var steps = await dbContext.LiveDemoRunSteps
            .AsNoTracking()
            .Where(candidate => candidate.TenantId == tenantId && candidate.RunId == run.Id)
            .OrderBy(candidate => candidate.Sequence)
            .Select(candidate => new LiveDemoRunStepResponse(
                candidate.Key,
                candidate.Sequence,
                candidate.PublicStage,
                candidate.Provider,
                candidate.Status.ToString(),
                candidate.EvidenceMode,
                candidate.AttemptCount,
                candidate.DurationMs,
                candidate.PublicSummary,
                candidate.PublicEvidenceReference,
                candidate.PublicErrorCode,
                candidate.PublicErrorMessage))
            .ToListAsync(cancellationToken);

        var result = run.Status == LiveDemoRunStatus.Completed
            ? await CreateResultAsync(run, tenantId, dbContext, cancellationToken)
            : null;
        var maxRetries = Math.Max(0, liveDemoOptions.Value.MaxRetriesPerRun);

        return Results.Ok(new LiveDemoRunResponse(
            run.Id,
            run.Status.ToString(),
            run.CurrentStepKey,
            run.CreatedAt,
            run.StartedAt,
            run.CompletedAt,
            run.TotalDurationMs,
            run.RetryCount,
            run.Status == LiveDemoRunStatus.Failed && run.RetryCount < maxRetries,
            run.PublicErrorCode,
            run.PublicErrorMessage,
            steps,
            result));
    }

    private static IResult GetLiveDemoCapabilities(
        ITenantContext tenantContext,
        IOptions<LiveDemoOptions> liveDemoOptions)
    {
        if (tenantContext.TenantId is not { })
        {
            return Results.Unauthorized();
        }

        var options = liveDemoOptions.Value;
        var enabled = options.Enabled && !string.IsNullOrWhiteSpace(options.OrganizationNumber);
        return Results.Ok(new LiveDemoCapabilitiesResponse(
            enabled,
            enabled,
            false,
            false,
            enabled));
    }

    private static async Task<IResult> RetryLiveDemoRun(
        Guid runId,
        ITenantContext tenantContext,
        IOptions<LiveDemoOptions> liveDemoOptions,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId || tenantContext.UserId is not { } userId)
        {
            return Results.Unauthorized();
        }

        var run = await dbContext.LiveDemoRuns.SingleOrDefaultAsync(
            candidate => candidate.Id == runId && candidate.TenantId == tenantId,
            cancellationToken);
        if (run is null)
        {
            return Results.NotFound(new { error = "Live demo run was not found." });
        }

        if (run.Status != LiveDemoRunStatus.Failed)
        {
            return Results.Conflict(new { error = "Only a failed live demo run can be retried." });
        }

        var maxRetries = Math.Max(0, liveDemoOptions.Value.MaxRetriesPerRun);
        if (run.RetryCount >= maxRetries)
        {
            return Results.Conflict(new { error = "The live demo run retry limit has been reached." });
        }

        var steps = await dbContext.LiveDemoRunSteps
            .Where(candidate => candidate.TenantId == tenantId && candidate.RunId == run.Id)
            .OrderBy(candidate => candidate.Sequence)
            .ToListAsync(cancellationToken);
        var firstFailedSequence = steps
            .Where(step => step.Status == LiveDemoRunStepStatus.Failed)
            .Select(step => (int?)step.Sequence)
            .Min();
        var now = DateTimeOffset.UtcNow;
        foreach (var step in steps.Where(step =>
                     firstFailedSequence is not null &&
                     step.Sequence >= firstFailedSequence &&
                     step.Status == LiveDemoRunStepStatus.Failed))
        {
            step.ResetForRetry(now);
        }

        run.QueueRetry(now, maxRetries);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditEventWriter.WriteAsync(
            new AuditEventRequest(
                tenantId,
                userId,
                "DemoSession",
                "LiveDemoRun",
                run.Id.ToString(),
                "LiveDemoRunRetried",
                null,
                $"{{\"retryCount\":{run.RetryCount}}}",
                null,
                null,
                httpContext.Response.Headers["X-Correlation-ID"].ToString()),
            cancellationToken);

        return Results.Accepted(
            $"/api/live-demo-runs/{run.Id}",
            new RetryLiveDemoRunResponse(
                run.Id,
                run.Status.ToString(),
                $"/api/live-demo-runs/{run.Id}",
                run.RetryCount,
                now));
    }

    private static async Task<LiveDemoRunResultResponse> CreateResultAsync(
        LiveDemoRun run,
        Guid tenantId,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var caseNumber = run.CaseId is { } caseId
            ? await dbContext.Cases
                .AsNoTracking()
                .Where(candidate => candidate.Id == caseId && candidate.TenantId == tenantId)
                .Select(candidate => candidate.CaseNumber)
                .SingleOrDefaultAsync(cancellationToken)
            : null;
        var auditEventCount = await dbContext.AuditEvents
            .AsNoTracking()
            .CountAsync(
                candidate => candidate.TenantId == tenantId && candidate.EntityId == run.Id.ToString(),
                cancellationToken);

        return new LiveDemoRunResultResponse(
            caseNumber,
            run.BrregMode,
            ShortenExternalReference(run.SharePointFolderItemId),
            ShortenExternalReference(run.SharePointFileItemId),
            ShortenExternalReference(run.ErpReceiptId),
            auditEventCount,
            $"/technical/live-runs/{run.Id}",
            run.CaseId is { } resultCaseId ? $"/cases/{resultCaseId}" : null,
            run.DocumentId is { } resultDocumentId ? $"/documents/{resultDocumentId}" : null,
            run.DocumentId is { } downloadDocumentId ? $"/api/documents/{downloadDocumentId}/download" : null,
            run.SharePointFileItemId is not null ? $"/technical/live-runs/{run.Id}#sharepoint" : null,
            $"/technical/live-runs/{run.Id}#audit");
    }

    private static string? ShortenExternalReference(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Length <= 16
            ? value
            : $"{value[..8]}…{value[^6..]}";

    private static IReadOnlyList<LiveDemoRunStep> CreatePendingSteps(Guid tenantId, Guid userId, Guid runId)
    {
        return
        [
            CreateStep("request-created", 1, "Mottatt", "Norvix WorkFlow Hub", "implemented"),
            CreateStep("brreg-checked", 2, "Kontrollert", "Brreg", "live-or-fallback"),
            CreateStep("case-created", 3, "Opprettet", "Norvix WorkFlow Hub", "implemented"),
            CreateStep("document-created", 4, "Opprettet", "Norvix WorkFlow Hub", "implemented"),
            CreateStep("sharepoint-synced", 5, "Synkronisert", "SharePoint simulator", "simulated-sharepoint"),
            CreateStep("erp-received", 6, "Synkronisert", "ERP demo receiver", "demo-receiver"),
            CreateStep("run-completed", 7, "Synkronisert", "Norvix WorkFlow Hub", "implemented")
        ];

        LiveDemoRunStep CreateStep(
            string key,
            int sequence,
            string publicStage,
            string provider,
            string evidenceMode) => new()
        {
            TenantId = tenantId,
            CreatedBy = userId,
            RunId = runId,
            Key = key,
            Sequence = sequence,
            PublicStage = publicStage,
            Provider = provider,
            EvidenceMode = evidenceMode
        };
    }

    private static bool TryGetBearerToken(HttpContext httpContext, out string token)
    {
        const string Prefix = "Bearer ";
        token = string.Empty;
        var authorization = httpContext.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = authorization[Prefix.Length..].Trim();
        return token.Length > 0;
    }
}
