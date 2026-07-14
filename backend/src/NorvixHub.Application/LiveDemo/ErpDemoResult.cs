namespace NorvixHub.Application.LiveDemo;

public enum ErpDemoResultStatus
{
    Received,
    Unauthorized,
    Conflict,
    Unavailable,
    Timeout,
    InvalidResponse,
    InvalidConfiguration
}

public sealed record ErpDemoResult(
    ErpDemoResultStatus Status,
    string? ReceiptId = null,
    bool Duplicate = false,
    DateTime? ReceivedAt = null)
{
    public bool IsSuccess => Status == ErpDemoResultStatus.Received;
    public bool IsRetryable => Status is ErpDemoResultStatus.Unavailable or ErpDemoResultStatus.Timeout;
}
