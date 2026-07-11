using NorvixHub.Application.Documents;
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

public static partial class DemoSessionEndpoints
{
    private static readonly (string Provider, string DisplayName, bool Connected)[] DemoIntegrations =
    [
        ("brreg", "Brreg – offentlig datakilde", true),
        ("microsoft-graph", "SharePoint / dokumentarkiv – simulert demo-adapter", false),
        ("tripletex", "Prosjekt/ERP – simulert demo-adapter", false),
        ("powerbi-fabric", "Power BI / rapportering – simulert demo-adapter", false)
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
        IFileStorage fileStorage,
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
        var seedIntakes = CreateSeedIntakes(tenantId, userId, now);
        dbContext.IntakeItems.AddRange(seedIntakes);
        dbContext.IntegrationConnections.AddRange(CreateSeedIntegrations(tenantId, userId, now));
        await AddSeedWorkspaceAsync(
            dbContext,
            fileStorage,
            tenantId,
            userId,
            seedIntakes[0],
            now,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/demo-sessions/{sessionId}",
            new CreateDemoSessionResponse(sessionId, tenantId, token, expiresAt));
    }

    private static IReadOnlyList<IntakeItem> CreateSeedIntakes(Guid tenantId, Guid userId, DateTimeOffset now)
    {
        return
        [
            new IntakeItem
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Source = IntakeSource.MockEmail,
                Subject = "Service og dokumentasjon – pumpestasjon 14",
                Body = "Hei,\nVi trenger service og dokumentasjon for pumpestasjon 14.\nKundereferanse: PO-10482.\nVedlagt ligger inspeksjonsnotat og bilder.\nBekreft mottak og opprett saken for driftsteamet.",
                CustomerName = "Kristiansand Kommune",
                OrganizationNumber = "963296746",
                Category = "Inspection",
                Urgency = "Normal",
                ReceivedAt = now.AddHours(-6)
            },
            new IntakeItem
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Source = IntakeSource.MockForm,
                Subject = "Skjema: hasteforespørsel om FDV-dokumentasjon",
                Body = "Webskjema fra kundeportal: FDV-dokumentasjon mangler for leveranse. Kunde ber om prioritet høy og bekreftet mottaker.",
                CustomerName = "Agder Energi Drift AS",
                Category = "Documentation",
                Urgency = "High",
                ReceivedAt = now.AddHours(-2)
            },
            new IntakeItem
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Source = IntakeSource.Api,
                Subject = "API: new maintenance order from field system",
                Body = "External field system submitted maintenance order MO-7781 with customer reference, asset ID and requested completion date.",
                CustomerName = "Setesdal Miljøservice AS",
                OrganizationNumber = "918273645",
                Category = "Maintenance",
                Urgency = "Normal",
                ReceivedAt = now.AddMinutes(-90)
            },
            new IntakeItem
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Source = IntakeSource.MockDocumentUpload,
                Subject = "Dokument: uploaded inspection attachment needs review",
                Body = "Uploaded PDF contains inspection notes, expiry date and customer reference. Needs classification before delivery.",
                CustomerName = "Arendal Eiendom KF",
                Category = "Document review",
                Urgency = "Low",
                ReceivedAt = now.AddMinutes(-55)
            },
            new IntakeItem
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Source = IntakeSource.Manual,
                Subject = "Manuell: phone request logged by operations",
                Body = "Operations user registered a phone request about missing delivery status and invoice reference for an existing service job.",
                CustomerName = "Lillesand Havnedrift AS",
                Category = "Customer follow-up",
                Urgency = "Normal",
                ReceivedAt = now.AddMinutes(-25)
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
