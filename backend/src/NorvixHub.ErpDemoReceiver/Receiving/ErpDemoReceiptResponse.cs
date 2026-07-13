namespace NorvixHub.ErpDemoReceiver.Receiving;

public sealed record ErpDemoReceiptResponse(
    string ReceiptId,
    string Status,
    bool Duplicate,
    DateTime ReceivedAt);
