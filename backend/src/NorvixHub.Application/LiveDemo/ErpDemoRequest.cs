namespace NorvixHub.Application.LiveDemo;

public sealed record ErpDemoRequest(
    Guid RunId,
    string CustomerReference,
    string CaseNumber,
    string DocumentReference,
    bool FailOnce);
