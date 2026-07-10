using NorvixHub.Application.Documents;
using NorvixHub.Domain.Audit;
using NorvixHub.Domain.Cases;
using NorvixHub.Domain.Customers;
using NorvixHub.Domain.Delivery;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Intake;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DemoSessionEndpoints
{
    private static async Task AddSeedWorkspaceAsync(
        NorvixHubDbContext dbContext,
        IFileStorage fileStorage,
        Guid tenantId,
        Guid userId,
        IntakeItem sourceIntake,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var customer = CreateSeedCustomer(tenantId, userId, now);
        var caseWorkspace = CreateSeedCase(tenantId, userId, customer.Id, sourceIntake.Id, now);
        sourceIntake.MarkConvertedToCase(caseWorkspace.Id, userId, now.AddHours(-5));
        var approvedDocument = await CreateApprovedSeedDocumentAsync(
            fileStorage,
            tenantId,
            userId,
            caseWorkspace.Id,
            now,
            cancellationToken);
        var package = CreateSeedPackage(tenantId, userId, caseWorkspace.Id, now);
        var packageItem = CreateSeedPackageItem(tenantId, userId, package.Id, approvedDocument.Document, now);
        var summaryDocument = await CreateSummarySeedDocumentAsync(
            fileStorage,
            tenantId,
            userId,
            caseWorkspace.Id,
            now,
            cancellationToken);

        package.MarkSummaryGenerated(summaryDocument.Document.Id, userId, now.AddMinutes(-45));

        dbContext.Customers.Add(customer);
        dbContext.Cases.Add(caseWorkspace);
        dbContext.IntakeAttachments.AddRange(CreateSeedIntakeAttachments(
            tenantId,
            userId,
            sourceIntake.Id,
            now));
        dbContext.CaseTasks.Add(CreateSeedTask(tenantId, userId, caseWorkspace.Id, now));
        dbContext.CaseNotes.Add(CreateSeedNote(tenantId, userId, caseWorkspace.Id, now));
        dbContext.Documents.AddRange(approvedDocument.Document, summaryDocument.Document);
        dbContext.DocumentVersions.AddRange(approvedDocument.Version, summaryDocument.Version);
        dbContext.DeliveryPackages.Add(package);
        dbContext.DeliveryPackageItems.Add(packageItem);
        dbContext.AuditEvents.AddRange(CreateSeedAuditTrail(
            tenantId,
            userId,
            sourceIntake,
            caseWorkspace,
            approvedDocument.Document,
            package,
            now));
    }

    private static IReadOnlyList<IntakeAttachment> CreateSeedIntakeAttachments(
        Guid tenantId,
        Guid userId,
        Guid intakeId,
        DateTimeOffset now)
    {
        return
        [
            new IntakeAttachment
            {
                TenantId = tenantId,
                CreatedBy = userId,
                CreatedAt = now.AddHours(-6),
                IntakeItemId = intakeId,
                OriginalFilename = "inspeksjonsnotat.pdf",
                ContentType = "application/pdf",
                SizeBytes = 184_320
            },
            new IntakeAttachment
            {
                TenantId = tenantId,
                CreatedBy = userId,
                CreatedAt = now.AddHours(-6),
                IntakeItemId = intakeId,
                OriginalFilename = "bilder-pumpestasjon-14.zip",
                ContentType = "application/zip",
                SizeBytes = 2_457_600
            }
        ];
    }

    private static Customer CreateSeedCustomer(Guid tenantId, Guid userId, DateTimeOffset now)
    {
        return new Customer
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = now.AddHours(-6),
            Name = "Kristiansand Kommune",
            OrganizationNumber = "963296746",
            PrimaryContactName = "Demo Kontakt",
            PrimaryContactEmail = "demo.kontakt@example.test",
            BrregDataJson = "{\"source\":\"demo\",\"status\":\"fictional seed based on public registry shape\"}",
            SourceUpdatedAt = now.AddHours(-6)
        };
    }

    private static CaseWorkspace CreateSeedCase(
        Guid tenantId,
        Guid userId,
        Guid customerId,
        Guid sourceIntakeId,
        DateTimeOffset now)
    {
        return new CaseWorkspace
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = now.AddHours(-5),
            CaseNumber = $"DEMO-{now:yyyyMMdd}-001",
            Title = "Service og dokumentasjon – pumpestasjon 14",
            Description = "Fiktiv demosak som viser serviceinntak, dokumentkontroll, leveringsgrunnlag og sporbarhet.",
            CustomerId = customerId,
            OwnerUserId = userId,
            DueDate = DateOnly.FromDateTime(now.AddDays(7).UtcDateTime),
            MissingInformationJson = "[\"Bekreft mottaker hos driftsteamet\"]",
            ExternalProjectId = "PO-10482",
            SourceIntakeItemId = sourceIntakeId
        };
    }

    private static CaseTask CreateSeedTask(Guid tenantId, Guid userId, Guid caseId, DateTimeOffset now)
    {
        return new CaseTask
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = now.AddHours(-4),
            CaseId = caseId,
            Title = "Kontroller klassifisering av inspeksjonsnotat",
            Description = "Bekreft dokumenttype og metadata før leveringsgrunnlaget ferdigstilles.",
            AssignedToUserId = userId,
            DueDate = DateOnly.FromDateTime(now.AddDays(2).UtcDateTime)
        };
    }

    private static CaseNote CreateSeedNote(Guid tenantId, Guid userId, Guid caseId, DateTimeOffset now)
    {
        return new CaseNote
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = now.AddHours(-3),
            CaseId = caseId,
            Body = "Demo: Brreg bruker et deterministisk snapshot. SharePoint, prosjekt/ERP og rapportering vises med demo-adaptere.",
            Visibility = "Internal"
        };
    }

    private static async Task<SeedDocument> CreateApprovedSeedDocumentAsync(
        IFileStorage fileStorage,
        Guid tenantId,
        Guid userId,
        Guid caseId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var approvedDocument = await CreateStoredDocumentAsync(
            fileStorage,
            tenantId,
            userId,
            now.AddHours(-3),
            "Godkjent inspeksjonsnotat – pumpestasjon 14",
            "godkjent-inspeksjonsnotat-pumpestasjon-14.pdf",
            "Godkjent inspeksjonsnotat for den fiktive demosaken.",
            cancellationToken);
        approvedDocument.Document.ApproveClassification(
            "Inspection report",
            DateOnly.FromDateTime(now.AddMonths(12).UtcDateTime),
            userId,
            now.AddHours(-2));
        approvedDocument.Document.LinkToCase(caseId, userId, now.AddHours(-2));
        return approvedDocument;
    }

    private static DeliveryPackage CreateSeedPackage(
        Guid tenantId,
        Guid userId,
        Guid caseId,
        DateTimeOffset now)
    {
        return new DeliveryPackage
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = now.AddHours(-1),
            CaseId = caseId,
            Title = "Leveringsgrunnlag – pumpestasjon 14"
        };
    }

    private static DeliveryPackageItem CreateSeedPackageItem(
        Guid tenantId,
        Guid userId,
        Guid packageId,
        DocumentRecord document,
        DateTimeOffset now)
    {
        return new DeliveryPackageItem
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = now.AddHours(-1),
            DeliveryPackageId = packageId,
            DocumentId = document.Id,
            DisplayName = document.Title
        };
    }

    private static async Task<SeedDocument> CreateSummarySeedDocumentAsync(
        IFileStorage fileStorage,
        Guid tenantId,
        Guid userId,
        Guid caseId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var summaryDocument = await CreateStoredDocumentAsync(
            fileStorage,
            tenantId,
            userId,
            now.AddMinutes(-45),
            "Leveringsgrunnlag – pumpestasjon 14 – sammendrag.pdf",
            "leveringsgrunnlag-pumpestasjon-14-sammendrag.pdf",
            "Generert fiktivt leveringssammendrag for demoarbeidsområdet.",
            cancellationToken);
        summaryDocument.Document.LinkToCase(caseId, userId, now.AddMinutes(-45));
        return summaryDocument;
    }

    private static async Task<SeedDocument> CreateStoredDocumentAsync(
        IFileStorage fileStorage,
        Guid tenantId,
        Guid userId,
        DateTimeOffset createdAt,
        string title,
        string filename,
        string body,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(CreateSeedPdfBytes(title, body));
        var stored = await fileStorage.SaveAsync(stream, filename, "application/pdf", cancellationToken);
        var document = new DocumentRecord
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = createdAt,
            Title = title
        };
        var version = new DocumentVersion
        {
            TenantId = tenantId,
            CreatedBy = userId,
            CreatedAt = createdAt,
            DocumentId = document.Id,
            VersionNumber = 1,
            BlobContainer = stored.Container,
            BlobName = stored.BlobName,
            OriginalFilename = filename,
            ContentType = "application/pdf",
            SizeBytes = stored.SizeBytes,
            Sha256Hash = stored.Sha256Hash,
            UploadedByUserId = userId
        };
        document.SetCurrentVersion(version.Id, userId, createdAt);
        return new SeedDocument(document, version);
    }

    private static IEnumerable<AuditEvent> CreateSeedAuditTrail(
        Guid tenantId,
        Guid userId,
        IntakeItem sourceIntake,
        CaseWorkspace caseWorkspace,
        DocumentRecord document,
        DeliveryPackage package,
        DateTimeOffset now)
    {
        return
        [
            CreateAudit(tenantId, userId, "IntakeItem", sourceIntake.Id, "IntakeCreated", now.AddHours(-6)),
            CreateAudit(tenantId, userId, "IntakeItem", sourceIntake.Id, "AiAnalysisRequested", now.AddHours(-5).AddMinutes(-40)),
            CreateAudit(tenantId, userId, "IntakeItem", sourceIntake.Id, "AiSuggestionApproved", now.AddHours(-5).AddMinutes(-20)),
            CreateAudit(tenantId, userId, "Customer", caseWorkspace.CustomerId!.Value, "CompanyDataChecked", now.AddHours(-5).AddMinutes(-10)),
            CreateAudit(tenantId, userId, "CaseWorkspace", caseWorkspace.Id, "CaseCreated", now.AddHours(-5)),
            CreateAudit(tenantId, userId, "DocumentRecord", document.Id, "DocumentUploaded", now.AddHours(-3)),
            CreateAudit(tenantId, userId, "DocumentRecord", document.Id, "DocumentClassificationApproved", now.AddHours(-2)),
            CreateAudit(tenantId, userId, "DocumentRecord", document.Id, "DocumentLinkedToCase", now.AddHours(-2)),
            CreateAudit(tenantId, userId, "DeliveryPackage", package.Id, "DeliveryPackageCreated", now.AddHours(-1)),
            CreateAudit(tenantId, userId, "DeliveryPackage", package.Id, "ReportingBasisUpdated", now.AddMinutes(-50)),
            CreateAudit(tenantId, userId, "DeliveryPackage", package.Id, "DeliveryPdfGenerated", now.AddMinutes(-45))
        ];
    }

    private static AuditEvent CreateAudit(
        Guid tenantId,
        Guid userId,
        string entityType,
        Guid entityId,
        string action,
        DateTimeOffset createdAt)
    {
        return new AuditEvent
        {
            TenantId = tenantId,
            ActorUserId = userId,
            ActorType = "User",
            EntityType = entityType,
            EntityId = entityId.ToString(),
            Action = action,
            CorrelationId = $"seed-{entityId:N}"[..37],
            CreatedAt = createdAt
        };
    }

    private sealed record SeedDocument(DocumentRecord Document, DocumentVersion Version);
}
