using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Application.SharePoint;
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
    IFileStorage fileStorage,
    ILiveDemoOrganizationResolver organizationResolver,
    ISharePointDocumentAdapterResolver sharePointAdapterResolver,
    IErpDemoClient erpDemoClient) : ILiveDemoRunProcessor
{
    public async Task ProcessAsync(Guid runId, CancellationToken cancellationToken)
    {
        await ProcessRequestCreatedAsync(runId, cancellationToken);
        await ProcessBrregCheckedAsync(runId, cancellationToken);
        await ProcessCaseCreatedAsync(runId, cancellationToken);
        await ProcessDocumentCreatedAsync(runId, cancellationToken);
        await ProcessSharePointSyncedAsync(runId, cancellationToken);
        await ProcessErpReceivedAsync(runId, cancellationToken);
        await ProcessRunCompletedAsync(runId, cancellationToken);
    }

    private Task ProcessRequestCreatedAsync(Guid runId, CancellationToken cancellationToken) =>
        ProcessInternalStepAsync(
            runId,
            "request-created",
            "Fiktiv henvendelse registrert.",
            "RUN-REQUEST",
            cancellationToken);

    private async Task ProcessBrregCheckedAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await dbContext.LiveDemoRuns.SingleOrDefaultAsync(candidate => candidate.Id == runId, cancellationToken)
            ?? throw new InvalidOperationException("Live demo run was not found.");
        if (run.Status is LiveDemoRunStatus.Completed or LiveDemoRunStatus.Failed)
        {
            return;
        }

        var step = await dbContext.LiveDemoRunSteps.SingleAsync(
            candidate => candidate.RunId == runId && candidate.Key == "brreg-checked",
            cancellationToken);
        if (step.Status == LiveDemoRunStepStatus.Completed)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        try
        {
            await using (var startTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                StartRunAndStep(run, step, now);
                await dbContext.SaveChangesAsync(cancellationToken);
                await startTransaction.CommitAsync(cancellationToken);
            }

            var resolution = await organizationResolver.ResolveAsync(run.OrganizationNumber, cancellationToken);
            await using (var resultTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                var completedAt = DateTimeOffset.UtcNow;
                var customer = await GetOrCreateBrregCustomerAsync(
                    run,
                    resolution,
                    completedAt,
                    cancellationToken);
                run.SetInternalArtifacts(null, customer.Id, null, null, null, completedAt);
                run.SetBrregEvidence(resolution.Mode, resolution.Organization.SourceUpdatedAt, completedAt);
                var summary = resolution.Mode == "live"
                    ? $"Firmadata kontrollert mot Brreg for {resolution.Organization.Name}."
                    : "Brreg var utilgjengelig; et tydelig merket fallback-snapshot ble brukt.";
                step.MarkCompleted(summary, resolution.Mode, completedAt);
                await dbContext.SaveChangesAsync(cancellationToken);
                await WriteAuditAsync(run, "LiveDemoStepCompleted", "brreg-checked", cancellationToken);
                await resultTransaction.CommitAsync(cancellationToken);
            }
        }
        catch (LiveDemoOrganizationResolutionException exception)
        {
            await using var failureTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await MarkFailedAsync(run, step, "brreg-checked", exception, cancellationToken);
            await failureTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await using var failureTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await MarkFailedAsync(run, step, "brreg-checked", exception, cancellationToken);
            await failureTransaction.CommitAsync(cancellationToken);
        }
    }

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

    private async Task ProcessSharePointSyncedAsync(Guid runId, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var run = await dbContext.LiveDemoRuns.SingleAsync(candidate => candidate.Id == runId, cancellationToken);
        var step = await dbContext.LiveDemoRunSteps.SingleAsync(candidate => candidate.RunId == runId && candidate.Key == "sharepoint-synced", cancellationToken);
        if (step.Status == LiveDemoRunStepStatus.Completed || run.Status is LiveDemoRunStatus.Completed or LiveDemoRunStatus.Failed) return;
        var document = await dbContext.Documents.SingleAsync(candidate => candidate.Id == run.DocumentId && candidate.TenantId == run.TenantId, cancellationToken);
        var version = await dbContext.DocumentVersions.SingleAsync(candidate => candidate.Id == document.CurrentVersionId && candidate.TenantId == run.TenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        try
        {
            StartRunAndStep(run, step, now);
            var result = await sharePointAdapterResolver.GetCurrent().SynchronizeAsync(new SharePointDocumentSyncRequest(
                run.TenantId, run.CreatedBy, run.CaseId!.Value, "Fiktiv live-demo kunde", run.CustomerReference,
                document.Id, version.Id, version.OriginalFilename, version.SizeBytes, "LiveDemoPdf", "Approved", null, null, run.Id), cancellationToken);
            if (!result.Succeeded || result.Item is null) throw new InvalidOperationException(result.PublicMessage);
            run.SetSharePointEvidence(
                CreateSafeReference(result.Item.DriveId),
                CreateSafeReference(result.Item.ParentPath),
                CreateSafeReference(result.Item.ExternalItemId),
                now);
            step.MarkCompleted(
                "Lokal SharePoint-simulator — ingen Microsoft 365-konto er tilkoblet.",
                result.Item.ExternalItemId,
                now);
            await dbContext.SaveChangesAsync(cancellationToken);
            await WriteAuditAsync(run, "LiveDemoStepCompleted", "sharepoint-synced", cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await MarkFailedAsync(run, step, "sharepoint-synced", exception, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

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
            var hasUnfinishedActiveSteps = await dbContext.LiveDemoRunSteps.AnyAsync(
                candidate => candidate.RunId == runId &&
                    candidate.Key != "run-completed" &&
                    candidate.Status != LiveDemoRunStepStatus.Completed &&
                    candidate.Status != LiveDemoRunStepStatus.Skipped,
                cancellationToken);
            if (hasUnfinishedActiveSteps)
            {
                throw new InvalidOperationException(
                    "A live demo run cannot complete while an active step is unfinished.");
            }
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

    private async Task ProcessErpReceivedAsync(Guid runId, CancellationToken cancellationToken)
    {
        await using (var startTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            var run = await dbContext.LiveDemoRuns.SingleAsync(candidate => candidate.Id == runId, cancellationToken);
            var step = await dbContext.LiveDemoRunSteps.SingleAsync(
                candidate => candidate.RunId == runId && candidate.Key == "erp-received",
                cancellationToken);
            if (step.Status is LiveDemoRunStepStatus.Completed or LiveDemoRunStepStatus.Skipped ||
                run.Status is LiveDemoRunStatus.Completed or LiveDemoRunStatus.Failed)
            {
                return;
            }

            StartRunAndStep(run, step, DateTimeOffset.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
            await startTransaction.CommitAsync(cancellationToken);
        }

        dbContext.ChangeTracker.Clear();
        try
        {
            var payloadSource = await dbContext.LiveDemoRuns
                .AsNoTracking()
                .Where(candidate => candidate.Id == runId)
                .Select(candidate => new
                {
                    candidate.Id,
                    candidate.TenantId,
                    candidate.CustomerReference,
                    candidate.CaseId,
                    candidate.DocumentId,
                    candidate.SimulateErpFailureOnce
                })
                .SingleAsync(cancellationToken);
            var caseNumber = await dbContext.Cases
                .AsNoTracking()
                .Where(candidate => candidate.Id == payloadSource.CaseId && candidate.TenantId == payloadSource.TenantId)
                .Select(candidate => candidate.CaseNumber)
                .SingleAsync(cancellationToken);
            var documentReference = await dbContext.DocumentVersions
                .AsNoTracking()
                .Where(candidate => candidate.DocumentId == payloadSource.DocumentId && candidate.TenantId == payloadSource.TenantId)
                .OrderByDescending(candidate => candidate.VersionNumber)
                .Select(candidate => candidate.OriginalFilename)
                .FirstAsync(cancellationToken);
            var result = await erpDemoClient.SendAsync(
                new ErpDemoRequest(
                    payloadSource.Id,
                    $"FICTIONAL-{payloadSource.CustomerReference}",
                    caseNumber,
                    documentReference,
                    payloadSource.SimulateErpFailureOnce),
                cancellationToken);
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.ReceiptId))
            {
                throw new ErpDemoProcessingException(result.Status);
            }

            await using var successTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var run = await dbContext.LiveDemoRuns.SingleAsync(candidate => candidate.Id == runId, cancellationToken);
            var step = await dbContext.LiveDemoRunSteps.SingleAsync(
                candidate => candidate.RunId == runId && candidate.Key == "erp-received",
                cancellationToken);
            var completedAt = DateTimeOffset.UtcNow;
            run.SetErpReceipt(result.ReceiptId, completedAt);
            step.MarkCompleted(
                result.Duplicate
                    ? "ERP-meldingen var allerede mottatt; samme kvittering ble brukt."
                    : "ERP-meldingen ble mottatt av Norvix ERP demo receiver.",
                result.ReceiptId,
                completedAt);
            await dbContext.SaveChangesAsync(cancellationToken);
            await WriteAuditAsync(run, "LiveDemoStepCompleted", "erp-received", cancellationToken);
            await successTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            dbContext.ChangeTracker.Clear();
            await using var failureTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var run = await dbContext.LiveDemoRuns.SingleAsync(candidate => candidate.Id == runId, cancellationToken);
            var step = await dbContext.LiveDemoRunSteps.SingleAsync(
                candidate => candidate.RunId == runId && candidate.Key == "erp-received",
                cancellationToken);
            await MarkFailedAsync(run, step, "erp-received", exception, cancellationToken);
            await failureTransaction.CommitAsync(cancellationToken);
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

        var customer = run.CustomerId is { } customerId
            ? await dbContext.Customers.SingleAsync(
                candidate => candidate.Id == customerId && candidate.TenantId == run.TenantId,
                cancellationToken)
            : throw new InvalidOperationException("Live demo Brreg customer was not created.");

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

    private async Task<Customer> GetOrCreateBrregCustomerAsync(
        LiveDemoRun run,
        LiveDemoOrganizationResolution resolution,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(
            candidate => candidate.TenantId == run.TenantId &&
                candidate.OrganizationNumber == resolution.Organization.OrganizationNumber,
            cancellationToken);
        var internalData = resolution.InternalRawJson ??
            $"{{\"source\":\"live-demo-fallback\",\"mode\":\"{resolution.Mode}\"}}";
        if (customer is null)
        {
            customer = new Customer
            {
                TenantId = run.TenantId,
                CreatedBy = run.CreatedBy,
                Name = resolution.Organization.Name,
                OrganizationNumber = resolution.Organization.OrganizationNumber,
                BrregDataJson = internalData,
                Source = $"LiveDemoBrreg:{resolution.Mode}",
                SourceUpdatedAt = resolution.Organization.SourceUpdatedAt
            };
            dbContext.Customers.Add(customer);
            return customer;
        }

        customer.Name = resolution.Organization.Name;
        customer.BrregDataJson = internalData;
        customer.Source = $"LiveDemoBrreg:{resolution.Mode}";
        customer.SourceUpdatedAt = resolution.Organization.SourceUpdatedAt;
        customer.MarkUpdated(run.CreatedBy, now);
        return customer;
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

    private static string CreateSafeReference(string value) =>
        "SP-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..12];

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

    private sealed class ErpDemoProcessingException(ErpDemoResultStatus status)
        : Exception($"ERP demo receiver returned {status}.");
}
