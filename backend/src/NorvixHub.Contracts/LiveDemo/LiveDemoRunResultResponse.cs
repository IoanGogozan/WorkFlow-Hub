namespace NorvixHub.Contracts.LiveDemo;

public sealed record LiveDemoRunResultResponse(
    string? CaseNumber,
    string? DocumentFileName,
    string? BrregMode,
    string? SharePointFolderReference,
    string? SharePointFileReference,
    string? ErpReceiptId,
    int? AuditEventCount,
    string EvidenceHref,
    string? CaseHref,
    string? DocumentHref,
    string? DocumentDownloadHref,
    string? SharePointEvidenceHref,
    string AuditHref);
