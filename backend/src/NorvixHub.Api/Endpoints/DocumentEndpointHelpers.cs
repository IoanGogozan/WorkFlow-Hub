using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Documents;
using NorvixHub.Domain.AI;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Reviews;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private static bool CanWriteDocuments(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or TenantRole.Admin or TenantRole.OperationsUser;
    }

    private static bool CanReviewDocuments(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or
            TenantRole.Admin or
            TenantRole.OperationsUser or
            TenantRole.Reviewer;
    }

    private static DocumentLink CreateCaseLink(
        DocumentRecord document,
        Guid caseId,
        ITenantContext tenantContext)
    {
        return new DocumentLink
        {
            TenantId = document.TenantId,
            CreatedBy = tenantContext.UserId,
            DocumentId = document.Id,
            EntityType = "Case",
            EntityId = caseId
        };
    }

    private static AiAnalysisRun CreateAnalysisRun(
        DocumentRecord document,
        IDocumentClassificationProvider classifier,
        DocumentClassificationSuggestion suggestion,
        DocumentClassificationResponse response)
    {
        return new AiAnalysisRun
        {
            TenantId = document.TenantId,
            EntityType = "Document",
            EntityId = document.Id,
            Provider = classifier.Provider,
            Model = classifier.Model,
            PromptVersion = classifier.PromptVersion,
            InputHash = Convert.ToHexString(SHA256.HashData(document.Id.ToByteArray())),
            OutputJson = JsonSerializer.Serialize(response),
            Confidence = suggestion.Confidence
        };
    }

    private static ReviewTask CreateReviewTask(
        DocumentRecord document,
        AiAnalysisRun run,
        ITenantContext tenantContext)
    {
        return new ReviewTask
        {
            TenantId = document.TenantId,
            CreatedBy = tenantContext.UserId,
            EntityType = "Document",
            EntityId = document.Id,
            ReviewType = "DocumentClassification",
            AiAnalysisRunId = run.Id
        };
    }

    private static async Task<DocumentVersion?> FindCurrentVersionAsync(
        DocumentRecord? document,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (document?.CurrentVersionId is not { } versionId)
        {
            return null;
        }

        return await dbContext.DocumentVersions.FindAsync(new object?[] { versionId }, cancellationToken);
    }

    private static async Task<ClassificationState?> FindReviewStateAsync(
        Guid documentId,
        Guid runId,
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var document = await FindDocumentAsync(documentId, tenantContext, dbContext, cancellationToken);
        var run = await dbContext.AiAnalysisRuns.SingleOrDefaultAsync(
            candidate => candidate.Id == runId && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);
        var task = await dbContext.ReviewTasks.SingleOrDefaultAsync(
            candidate => candidate.AiAnalysisRunId == runId && candidate.TenantId == tenantContext.TenantId,
            cancellationToken);

        return document is null || run is null || task is null ? null : new ClassificationState(document, run, task);
    }

    private sealed record ClassificationState(DocumentRecord Document, AiAnalysisRun Run, ReviewTask Task);
}
