namespace NorvixHub.Contracts.Documents;

public sealed record DocumentVersionResponse(
    Guid Id,
    Guid DocumentId,
    int VersionNumber,
    string OriginalFilename,
    string ContentType,
    long SizeBytes,
    string Sha256Hash,
    DateTimeOffset CreatedAt);

