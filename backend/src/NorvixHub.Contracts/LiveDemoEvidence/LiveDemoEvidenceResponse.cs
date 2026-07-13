namespace NorvixHub.Contracts.LiveDemoEvidence;

public sealed record LiveDemoEvidenceResponse(
    LiveDemoEvidenceRunResponse Run,
    LiveDemoEvidenceRequestResponse? Request,
    LiveDemoEvidenceBrregResponse? Brreg,
    LiveDemoEvidenceCaseResponse? Case,
    LiveDemoEvidenceDocumentResponse? Document,
    LiveDemoEvidenceSharePointResponse? SharePoint,
    LiveDemoEvidenceErpResponse? Erp,
    IReadOnlyList<LiveDemoEvidenceAuditEventResponse> AuditEvents,
    LiveDemoEvidenceLinksResponse Links);
