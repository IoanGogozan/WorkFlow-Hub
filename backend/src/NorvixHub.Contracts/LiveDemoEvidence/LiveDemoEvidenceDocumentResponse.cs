namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceDocumentResponse(
    Guid DocumentId,
    string Title,
    string FileName,
    long SizeBytes,
    string ContentType,
    int VersionNumber,
    string? ContentHash,
    DateTimeOffset CreatedAt,
    string DocumentHref,
    string DownloadHref);
