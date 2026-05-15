using NorvixHub.Domain.Common;

namespace NorvixHub.Domain.Cases;

public sealed class CaseNote : TenantScopedEntity
{
    public Guid CaseId { get; init; }
    public required string Body { get; init; }
    public string Visibility { get; init; } = "Internal";
}

