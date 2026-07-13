namespace NorvixHub.ErpDemoReceiver.Persistence;

public sealed class ErpDemoReceipt
{
    private ErpDemoReceipt()
    {
    }

    public ErpDemoReceipt(
        Guid id,
        string idempotencyKey,
        string payloadHash,
        string customerReference,
        string caseNumber,
        string documentReference)
    {
        Id = id;
        IdempotencyKey = idempotencyKey;
        PayloadHash = payloadHash;
        CustomerReference = customerReference;
        CaseNumber = caseNumber;
        DocumentReference = documentReference;
    }

    public Guid Id { get; private set; }
    public string? ExternalReceiptId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string PayloadHash { get; private set; } = string.Empty;
    public string CustomerReference { get; private set; } = string.Empty;
    public string CaseNumber { get; private set; } = string.Empty;
    public string DocumentReference { get; private set; } = string.Empty;
    public DateTime? ReceivedAt { get; private set; }
    public int AttemptCount { get; private set; }
    public bool FailOnceTriggered { get; private set; }

    public void RegisterAttempt() => AttemptCount++;

    public void MarkFailOnceTriggered() => FailOnceTriggered = true;

    public void MarkReceived(string externalReceiptId, DateTime receivedAt)
    {
        ExternalReceiptId = externalReceiptId;
        ReceivedAt = receivedAt;
    }
}
