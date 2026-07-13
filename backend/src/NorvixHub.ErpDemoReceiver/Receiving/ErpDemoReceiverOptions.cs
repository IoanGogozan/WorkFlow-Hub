namespace NorvixHub.ErpDemoReceiver.Receiving;

public sealed class ErpDemoReceiverOptions
{
    public const string SectionName = "ErpDemoReceiver";

    public string SigningSecret { get; set; } = string.Empty;
    public int MaximumTimestampSkewSeconds { get; set; } = 300;
    public bool EnableFailOnce { get; set; }
}
