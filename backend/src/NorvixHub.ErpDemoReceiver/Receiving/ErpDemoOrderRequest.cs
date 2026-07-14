namespace NorvixHub.ErpDemoReceiver.Receiving;

public sealed record ErpDemoOrderRequest(
    string CustomerReference,
    string CaseNumber,
    string DocumentReference);
