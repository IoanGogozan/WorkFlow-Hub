namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceSharePointResponse(
    string Mode,
    string SiteName,
    string LibraryName,
    string FolderPath,
    string FolderId,
    string FileId,
    string FileName,
    int Version,
    string ETag,
    IReadOnlyDictionary<string, string> Metadata,
    IReadOnlyList<LiveDemoEvidenceSharePointOperationResponse> Operations,
    string TechnicalSharePointHref);
