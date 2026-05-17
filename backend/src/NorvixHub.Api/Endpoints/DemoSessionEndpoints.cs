using NorvixHub.Api.Auth;
using NorvixHub.Api.RateLimiting;
using NorvixHub.Contracts.Auth;
using NorvixHub.Domain.Demo;
using NorvixHub.Domain.Integrations;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class DemoSessionEndpoints
{
    private static readonly (string Provider, string DisplayName, bool Connected)[] DemoIntegrations =
    [
        ("brreg", "Bronnoysundregistrene", true),
        ("microsoft-graph", "Microsoft Graph / SharePoint mock", false),
        ("tripletex", "Tripletex Accounting mock", false),
        ("powerbi-fabric", "Power BI / Fabric mock", false)
    ];

    public static IEndpointRouteBuilder MapDemoSessionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/demo-sessions", CreateDemoSession)
            .RequireRateLimiting(PublicDemoRateLimiting.DemoSessionCreationPolicy)
            .WithName("CreateDemoSession");
        return app;
    }

    private static async Task<IResult> CreateDemoSession(
        NorvixHubDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var sessionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = DemoToken.Create();
        var expiresAt = now.AddHours(24);
        var slugSuffix = sessionId.ToString("N")[..8];

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Agder Drift & Service AS",
            Slug = $"agder-drift-demo-{slugSuffix}",
            OrganizationNumber = "999999999"
        };
        var user = new UserProfile
        {
            Id = userId,
            DisplayName = "Demo Visitor",
            Email = $"demo.{slugSuffix}@workflow-demo.example"
        };
        var membership = new TenantMembership
        {
            TenantId = tenantId,
            UserId = userId,
            Role = TenantRole.TenantOwner
        };
        var session = new DemoSession
        {
            Id = sessionId,
            TenantId = tenantId,
            UserId = userId,
            TokenHash = DemoToken.Hash(token),
            CreatedAt = now,
            ExpiresAt = expiresAt,
            IpHash = DemoToken.HashOptional(httpContext.Connection.RemoteIpAddress?.ToString()),
            UserAgentHash = DemoToken.HashOptional(httpContext.Request.Headers.UserAgent.ToString())
        };

        dbContext.Tenants.Add(tenant);
        dbContext.Users.Add(user);
        dbContext.TenantMemberships.Add(membership);
        dbContext.DemoSessions.Add(session);
        dbContext.IntakeItems.AddRange(CreateSeedIntakes(tenantId, userId, now));
        dbContext.IntegrationConnections.AddRange(CreateSeedIntegrations(tenantId, userId, now));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/demo-sessions/{sessionId}",
            new CreateDemoSessionResponse(sessionId, tenantId, token, expiresAt));
    }

    private static IEnumerable<IntakeItem> CreateSeedIntakes(Guid tenantId, Guid userId, DateTimeOffset now)
    {
        return
        [
            new IntakeItem
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Source = IntakeSource.Manual,
                Subject = "Service request - pump station inspection",
                Body = "Customer asks for inspection documentation and delivery package for a municipal pump station.",
                CustomerName = "Kristiansand Kommune",
                OrganizationNumber = "963296746",
                Category = "Inspection",
                Urgency = "Normal",
                ReceivedAt = now.AddHours(-4)
            },
            new IntakeItem
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Source = IntakeSource.MockEmail,
                Subject = "Missing documentation for maintenance case",
                Body = "Operations team needs approved document classification before sending the delivery package.",
                CustomerName = "Agder Energi Drift AS",
                Category = "Documentation",
                Urgency = "High",
                ReceivedAt = now.AddHours(-2)
            }
        ];
    }

    private static IEnumerable<IntegrationConnection> CreateSeedIntegrations(
        Guid tenantId,
        Guid userId,
        DateTimeOffset now)
    {
        foreach (var integration in DemoIntegrations)
        {
            var connection = new IntegrationConnection
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Provider = integration.Provider,
                DisplayName = integration.DisplayName
            };

            if (integration.Connected)
            {
                connection.Connect("{}", userId, now);
            }

            yield return connection;
        }
    }
}
