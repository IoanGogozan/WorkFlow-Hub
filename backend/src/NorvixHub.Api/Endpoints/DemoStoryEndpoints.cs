using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.DemoStory;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class DemoStoryEndpoints
{
    private const string MissingStoryError = "Demo story is not available.";

    public static IEndpointRouteBuilder MapDemoStoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/demo-story", GetDemoStory)
            .WithName("GetDemoStory");

        return app;
    }

    private static async Task<IResult> GetDemoStory(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Results.Unauthorized();
        }

        var caseWorkspace = await dbContext.Cases
            .AsNoTracking()
            .Where(candidate => candidate.TenantId == tenantId && candidate.SourceIntakeItemId != null)
            .OrderBy(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (caseWorkspace?.SourceIntakeItemId is not { } intakeId)
        {
            return MissingStory();
        }

        var intake = await dbContext.IntakeItems
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == tenantId && candidate.Id == intakeId,
                cancellationToken);
        var customer = caseWorkspace.CustomerId is { } customerId
            ? await dbContext.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    candidate => candidate.TenantId == tenantId && candidate.Id == customerId,
                    cancellationToken)
            : null;
        var deliveryPackage = await dbContext.DeliveryPackages
            .AsNoTracking()
            .Where(candidate => candidate.TenantId == tenantId && candidate.CaseId == caseWorkspace.Id)
            .OrderBy(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (intake is null || customer is null || deliveryPackage is null)
        {
            return MissingStory();
        }

        var attachments = await dbContext.IntakeAttachments
            .AsNoTracking()
            .Where(candidate => candidate.TenantId == tenantId && candidate.IntakeItemId == intake.Id)
            .OrderBy(candidate => candidate.CreatedAt)
            .Select(candidate => candidate.OriginalFilename)
            .ToListAsync(cancellationToken);
        var documents = await dbContext.Documents
            .AsNoTracking()
            .Where(candidate => candidate.TenantId == tenantId && candidate.CaseId == caseWorkspace.Id)
            .OrderBy(candidate => candidate.CreatedAt)
            .Select(candidate => new { candidate.Id, candidate.Title })
            .ToListAsync(cancellationToken);
        var integrations = await dbContext.IntegrationConnections
            .AsNoTracking()
            .Where(candidate => candidate.TenantId == tenantId)
            .OrderBy(candidate => candidate.DisplayName)
            .Select(candidate => new
            {
                candidate.Provider,
                candidate.DisplayName,
                Status = candidate.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var evidenceEntityIds = documents
            .Select(document => document.Id.ToString())
            .Append(intake.Id.ToString())
            .Append(caseWorkspace.Id.ToString())
            .Append(deliveryPackage.Id.ToString())
            .ToList();
        var auditEventCount = await dbContext.AuditEvents
            .AsNoTracking()
            .CountAsync(
                candidate => candidate.TenantId == tenantId &&
                    evidenceEntityIds.Contains(candidate.EntityId),
                cancellationToken);

        var primaryDocument = documents.FirstOrDefault();
        var links = new DemoStoryTechnicalLinksResponse(
            $"/intakes/{intake.Id}",
            $"/cases/{caseWorkspace.Id}",
            primaryDocument is null ? null : $"/documents/{primaryDocument.Id}",
            $"/delivery-packages/{deliveryPackage.Id}",
            "/integrations");
        var response = new DemoStoryResponse(
            "pump-station-service",
            new DemoStoryRequestResponse(
                intake.Source.ToString(),
                GetFictionalSender(intake.Source.ToString()),
                intake.Subject,
                intake.Body,
                customer.Name,
                intake.OrganizationNumber,
                caseWorkspace.ExternalProjectId,
                attachments,
                intake.ReceivedAt),
            new DemoStoryOutcomeResponse(
                caseWorkspace.CaseNumber,
                caseWorkspace.Title,
                customer.Name,
                documents.Count,
                deliveryPackage.Title,
                deliveryPackage.Status.ToString(),
                auditEventCount),
            CreateEvidenceSteps(documents.Count, auditEventCount, links),
            integrations.Select(integration => new DemoStoryIntegrationResponse(
                integration.Provider,
                integration.DisplayName,
                GetIntegrationMode(integration.Provider),
                integration.Status,
                GetIntegrationExplanation(integration.Provider))).ToList(),
            links);

        return Results.Ok(response);
    }

    private static IReadOnlyList<DemoStoryEvidenceStepResponse> CreateEvidenceSteps(
        int documentCount,
        int auditEventCount,
        DemoStoryTechnicalLinksResponse links)
    {
        return
        [
            new("email-received", 1, "E-post mottatt", "Kilde og vedlegg er registrert.",
                "E-post", "implemented", "Henvendelsen finnes i inntaket", links.IntakeHref),
            new("data-validated", 2, "Data strukturert og kontrollert",
                "Kunde, referanse, kategori og manglende informasjon er identifisert.",
                "WorkFlow Hub", "implemented", "Strukturerte inntaksdata", links.IntakeHref),
            new("company-checked", 3, "Firmadata kontrollert",
                "Organisasjonsinformasjon er knyttet til saken.", "Brreg",
                "public-data-capable", "Offentlig datakilde / demo-snapshot", links.CaseHref),
            new("case-created", 4, "Sak opprettet",
                "Saken er opprettet med kunde, referanse og ansvarlig.", "Prosjekt/ERP",
                "implemented", "Opprettet sak", links.CaseHref),
            new("documents-linked", 5, "Dokumentstruktur opprettet",
                "Dokumenter er lagret, klassifisert og knyttet til saken.", "Dokumentarkiv",
                "implemented", $"{documentCount} tilknyttede dokumenter", links.PrimaryDocumentHref),
            new("delivery-updated", 6, "Rapportering og leveringsgrunnlag oppdatert",
                "Status og leveringsgrunnlag er samlet uten ny registrering.", "Rapportering",
                "demo-adapter", "Leveringspakke opprettet", links.DeliveryPackageHref),
            new("audit-stored", 7, "Sporbarhet lagret",
                "Viktige handlinger er synlige i hendelsesloggen.", "Hendelseslogg",
                "implemented", $"{auditEventCount} sporbare hendelser", links.CaseHref)
        ];
    }

    private static string GetFictionalSender(string source) =>
        source.Equals("MockEmail", StringComparison.OrdinalIgnoreCase)
            ? "service@kristiansand.example.test"
            : "Fiktiv demokilde";

    private static string GetIntegrationMode(string provider) =>
        provider.Equals("brreg", StringComparison.OrdinalIgnoreCase)
            ? "public-data-capable"
            : "demo-adapter";

    private static string GetIntegrationExplanation(string provider) =>
        provider.Equals("brreg", StringComparison.OrdinalIgnoreCase)
            ? "Offentlig datakilde; demoen kan bruke et deterministisk snapshot."
            : "Viser integrasjonsmønsteret uten å sende data til et ekte kundesystem.";

    private static IResult MissingStory() => Results.NotFound(new { error = MissingStoryError });
}
