namespace NorvixHub.Infrastructure.LiveDemo;

public sealed class ErpDemoOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "http://erp-receiver:8080/";
    public string SigningSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
}
