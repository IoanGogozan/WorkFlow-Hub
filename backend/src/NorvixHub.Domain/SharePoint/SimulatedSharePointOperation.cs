using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.SharePoint;

public sealed class SimulatedSharePointOperation : TenantScopedEntity
{
    public Guid? IntegrationSyncRunId { get; init; }
    public Guid? LiveDemoRunId { get; init; }
    public Guid? DocumentId { get; init; }
    public Guid? DocumentVersionId { get; init; }
    public required string Operation { get; init; }
    public required string HttpMethod { get; init; }
    public required string Target { get; init; }
    public string? RequestSummaryJson { get; init; }
    public string? ResponseSummaryJson { get; init; }
    public int StatusCode { get; init; }
    public bool Succeeded { get; init; }
    public long DurationMilliseconds { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}
