using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Domain.Cases;
using NorvixHub.Domain.Customers;
using NorvixHub.Domain.Delivery;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Infrastructure.LiveDemo;

public sealed class LiveDemoRunProcessor(
    NorvixHubDbContext dbContext,
    IAuditEventWriter auditEventWriter,
    IDemoPdfGenerator demoPdfGenerator,
    IFileStorage fileStorage) : ILiveDemoRunProcessor
{
    public async Task ProcessAsync(Guid runId, CancellationToken cancellationToken)
    {
        await ProcessRequestCreatedAsync(runId, cancellationToken);
        await ProcessCaseCreatedAsync(runId, cancellationToken);
        await ProcessDocumentCreatedAsync(runId, cancellationToken);
        await ProcessRunCompletedAsync(runId, cancellationToken);
    }

    private Task ProcessRequestCreatedAsync(Guid runId, CancellationToken cancellationToken) =>
        ProcessInternalStepAsync(
            runId,
            "request-created",
            "Fiktiv henvendelse registrert.",
            "RUN-REQUEST",
            cancellationToken);

    private Task ProcessCaseCreatedAsync(Guid runId, CancellationToken cancellationToken) =>
        ProcessInternalStepAsync(
            runId,
            "case-created",
            "Internt sakssteg gjennomført.",
            "INTERNAL-CASE",
            cancellationToken);

    private Task ProcessDocumentCreatedAsync(Guid runId, CancellationToken cancellationToken) =>
        ProcessInternalStepAsync(
            runId,
            "document-created",
            "Internt dokumentsteg gjennomført.",
            "INTERNAL-DOCUMENT",
            cancellationToken);

    private async Task ProcessRunCompletedAsync(Guid runId, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var run = await dbContext.LiveDemoRuns.SingleOrDefaultAsync(candidate => candidate.Id == runId, cancellationToken)
            ?? throw new InvalidOperationException("Live demo run was not found.");
        if (run.Status is LiveDemoRunStatus.Completed or LiveDemoRunStatus.Failed)
        {
            return;
        }

        var step = await dbContext.LiveDemoRunSteps.SingleAsync(
            candidate => candidate.RunId == runId && candidate.Key == "run-completed",
            cancellationToken);
        var now = DateTimeOffset.UtcNow;
        try
        {
            StartRunAndStep(run, step, now);
            step.MarkCompleted("Interne steg er registrert for denne kjøringen.", "RUN-COMPLETED", now);
            run.MarkCompleted(now);
            await dbContext.SaveChangesAsync(cancellationToken);
            await WriteAuditAsync(run, "LiveDemoStepCompleted", "run-completed", cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await MarkFailedAsync(run, step, "run-completed", exception, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private async Task ProcessInternalStepAsync(
        Guid runId,
        string stepKey,
        string publicSummary,
        string publicEvidenceReference,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var run = await dbContext.LiveDemoRuns.SingleOrDefaultAsync(candidate => candidate.Id == runId, cancellationToken)
            ?? throw new InvalidOperationException("Live demo run was not found.");
        if (run.Status is LiveDemoRunStatus.Completed or LiveDemoRunStatus.Failed)
        {
            return;
        }

        var step = await dbContext.LiveDemoRunSteps.SingleAsync(
            candidate => candidate.RunId == runId && candidate.Key == stepKey,
            cancellationToken);
        if (step.Status == LiveDemoRunStepStatus.Completed)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        try
        {
            StartRunAndStep(run, step, now);
            await CreateArtifactsForStepAsync(run, stepKey, now, cancellationToken);
            step.MarkCompleted(publicSummary, publicEvidenceReference, now);
            await dbContext.SaveChangesAsync(cancellationToken);
            await WriteAuditAsync(run, "LiveDemoStepCompleted", stepKey, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await MarkFailedAsync(run, step, stepKey, exception, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private static void StartRunAndStep(LiveDemoRun run, LiveDemoRunStep step, DateTimeOffset now)
    {
        if (run.Status == LiveDemoRunStatus.Queued)
        {
            run.MarkRunning(step.Key, now);
        }
        else
        {
            run.SetCurrentStep(step.Key, now);
        }

        step.MarkRunning(now);
    }

    private async Task CreateArtifactsForStepAsync(
        LiveDemoRun run,
        string stepKey,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        switch (stepKey)
        {
            case "request-created":
                await CreateIntakeAsync(run, now, cancellationToken);
                break;
            case "case-created":
                await CreateCustomerAndCaseAsync(run, now, cancellationToken);
                break;
            case "document-created":
                await CreateDocumentAndDeliveryPackageAsync(run, now, cancellationToken);
                break;
        }
    }

    private async Task CreateIntakeAsync(LiveDemoRun run, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (run.IntakeItemId is not null)
        {
            return;
        }

        var intake = new IntakeItem
        {
            TenantId = run.TenantId,
            CreatedBy = run.CreatedBy,
            Source = IntakeSource.MockForm,
            Subject = run.RequestTitle,
            Body = run.RequestBody,
            CustomerName = "Fiktiv live-demo kunde",
            OrganizationNumber = run.OrganizationNumber,
            Category = "Live demo",
            Urgency = "Normal",
            ReceivedAt = now
        };
        dbContext.IntakeItems.Add(intake);
        run.SetInternalArtifacts(intake.Id, null, null, null, null, now);
        await Task.CompletedTask;
    }

    private async Task CreateCustomerAndCaseAsync(
        LiveDemoRun run,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var intake = run.IntakeItemId is { } intakeId
            ? await dbContext.IntakeItems.SingleAsync(
                candidate => candidate.Id == intakeId && candidate.TenantId == run.TenantId,
                cancellationToken)
            : throw new InvalidOperationException("Live demo intake was not created.");

        Customer? customer = null;
        if (run.CustomerId is { } customerId)
        {
            customer = await dbContext.Customers.SingleAsync(
                candidate => candidate.Id == customerId && candidate.TenantId == run.TenantId,
                cancellationToken);
        }
        else
        {
            customer = await dbContext.Customers.SingleOrDefaultAsync(
                candidate => candidate.TenantId == run.TenantId && candidate.OrganizationNumber == run.OrganizationNumber,
                cancellationToken);
            if (customer is null)
            {
                customer = new Customer
                {
                    TenantId = run.TenantId,
                    CreatedBy = run.CreatedBy,
                    Name = "Fiktiv live-demo kunde",
                    OrganizationNumber = run.OrganizationNumber,
                    BrregDataJson = "{\"source\":\"fictional-live-demo\"}",
                    Source = "LiveDemo",
                    SourceUpdatedAt = now
                };
                dbContext.Customers.Add(customer);
            }
        }

        CaseWorkspace? caseWorkspace = null;
        if (run.CaseId is { } caseId)
        {
            caseWorkspace = await dbContext.Cases.SingleAsync(
                candidate => candidate.Id == caseId && candidate.TenantId == run.TenantId,
                cancellationToken);
        }
        else
        {
            var caseNumber = CreateCaseNumber(run);
            caseWorkspace = await dbContext.Cases.SingleOrDefaultAsync(
                candidate => candidate.TenantId == run.TenantId && candidate.CaseNumber == caseNumber,
                cancellationToken);
            if (caseWorkspace is null)
            {
                caseWorkspace = new CaseWorkspace
                {
                    TenantId = run.TenantId,
                    CreatedBy = run.CreatedBy,
                    CaseNumber = caseNumber,
                    Title = run.RequestTitle,
                    Description = run.RequestBody,
                    CustomerId = customer.Id,
                    OwnerUserId = run.CreatedBy,
                    SourceIntakeItemId = intake.Id,
                    ExternalProjectId = run.CustomerReference
                };
                dbContext.Cases.Add(caseWorkspace);
                intake.MarkConvertedToCase(
                    caseWorkspace.Id,
                    run.CreatedBy ?? throw new InvalidOperationException("Live demo run has no creator."),
                    now);
            }
        }

        run.SetInternalArtifacts(intake.Id, customer.Id, caseWorkspace.Id, null, null, now);
    }

    private async Task CreateDocumentAndDeliveryPackageAsync(
        LiveDemoRun run,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (run.CaseId is not { } caseId || run.CustomerId is not { } customerId)
        {
            throw new InvalidOperationException("Live demo case artifacts were not created.");
        }

        DocumentRecord? document = null;
        if (run.DocumentId is { } documentId)
        {
            document = await dbContext.Documents.SingleAsync(
                candidate => candidate.Id == documentId && candidate.TenantId == run.TenantId,
                cancellationToken);
        }
        else
        {
            var filename = $"live-demo-{run.Id:N}.pdf";
            var pdfBytes = demoPdfGenerator.Generate(
                $"Fiktiv live-demo dokumentasjon {run.CustomerReference}",
                "Fiktive data. Dette dokumentet ble opprettet for Norvix live-demo.");
            await using var stream = new MemoryStream(pdfBytes);
            var stored = await fileStorage.SaveAsync(stream, filename, "application/pdf", cancellationToken);
            document = new DocumentRecord
            {
                TenantId = run.TenantId,
                CreatedBy = run.CreatedBy,
                Title = $"Live-demo dokumentasjon {run.CustomerReference}",
                CustomerId = customerId
            };
            document.LinkToCase(caseId, run.CreatedBy, now);
            var version = new DocumentVersion
            {
                TenantId = run.TenantId,
                CreatedBy = run.CreatedBy,
                DocumentId = document.Id,
                VersionNumber = 1,
                BlobContainer = stored.Container,
                BlobName = stored.BlobName,
                OriginalFilename = filename,
                ContentType = "application/pdf",
                SizeBytes = stored.SizeBytes,
                Sha256Hash = stored.Sha256Hash,
                UploadedByUserId = run.CreatedBy
            };
            document.SetCurrentVersion(version.Id, run.CreatedBy, now);
            dbContext.Documents.Add(document);
            dbContext.DocumentVersions.Add(version);
            dbContext.DocumentLinks.Add(new DocumentLink
            {
                TenantId = run.TenantId,
                CreatedBy = run.CreatedBy,
                DocumentId = document.Id,
                EntityType = "CaseWorkspace",
                EntityId = caseId
            });
        }

        DeliveryPackage? package = null;
        if (run.DeliveryPackageId is { } packageId)
        {
            package = await dbContext.DeliveryPackages.SingleAsync(
                candidate => candidate.Id == packageId && candidate.TenantId == run.TenantId,
                cancellationToken);
        }
        else
        {
            package = new DeliveryPackage
            {
                TenantId = run.TenantId,
                CreatedBy = run.CreatedBy,
                CaseId = caseId,
                Title = $"Live-demo leveringsgrunnlag {run.CustomerReference}"
            };
            package.MarkSummaryGenerated(document.Id, run.CreatedBy, now);
            dbContext.DeliveryPackages.Add(package);
        }

        run.SetInternalArtifacts(null, null, caseId, document.Id, package.Id, now);
    }

    private static string CreateCaseNumber(LiveDemoRun run) =>
        $"LIVE-{run.CreatedAt:yyyy}-{run.Id:N}"[..18].ToUpperInvariant();

    private async Task MarkFailedAsync(
        LiveDemoRun run,
        LiveDemoRunStep step,
        string stepKey,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        if (step.Status == LiveDemoRunStepStatus.Pending)
        {
            StartRunAndStep(run, step, now);
        }

        if (step.Status == LiveDemoRunStepStatus.Running)
        {
            step.MarkFailed("RUN_PROCESSING_FAILED", "Live-demoen kunne ikke fullføres.", now);
        }

        if (run.Status == LiveDemoRunStatus.Running)
        {
            run.MarkFailed("RUN_PROCESSING_FAILED", "Live-demoen kunne ikke fullføres.", now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(run, "LiveDemoStepFailed", stepKey, cancellationToken);
    }

    private Task WriteAuditAsync(
        LiveDemoRun run,
        string action,
        string stepKey,
        CancellationToken cancellationToken) =>
        auditEventWriter.WriteAsync(
            new AuditEventRequest(
                run.TenantId,
                run.CreatedBy,
                "LiveDemoProcessor",
                "LiveDemoRun",
                run.Id.ToString(),
                action,
                null,
                $"{{\"stepKey\":\"{stepKey}\"}}",
                null,
                null,
                run.CorrelationId),
            cancellationToken);
}
