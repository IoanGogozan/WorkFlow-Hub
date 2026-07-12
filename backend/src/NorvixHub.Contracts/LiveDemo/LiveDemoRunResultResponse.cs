namespace NorvixHub.Contracts.LiveDemo;

public sealed record LiveDemoRunResultResponse(
    string? CaseNumber,
    string? BrregMode,
    string? SharePointFolderReference,
    string? SharePointFileReference,
    string? ErpReceiptId,
    int? AuditEventCount);
