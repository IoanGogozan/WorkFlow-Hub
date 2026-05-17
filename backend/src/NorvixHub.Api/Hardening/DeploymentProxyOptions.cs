namespace NorvixHub.Api.Hardening;

public sealed class DeploymentProxyOptions
{
    public bool ForwardedHeadersEnabled { get; set; } = true;
    public bool EnforceHttps { get; set; }
    public int HttpsPort { get; set; } = 443;
    public int ForwardLimit { get; set; } = 1;
    public List<string> KnownProxies { get; set; } = [];
    public List<string> KnownNetworks { get; set; } = [];
    public bool AllowUnknownProxies { get; set; }
}
