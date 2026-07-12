using Microsoft.Extensions.Options;
using NorvixHub.Application.SharePoint;

namespace NorvixHub.Infrastructure.SharePoint;

public sealed class SharePointDocumentAdapterResolver(
    IOptions<SharePointOptions> options,
    SimulatedSharePointDocumentAdapter simulatedAdapter,
    MicrosoftGraphSharePointDocumentAdapter microsoftGraphAdapter) : ISharePointDocumentAdapterResolver
{
    public ISharePointDocumentAdapter GetCurrent() => options.Value.Mode.Trim() switch
    {
        "" or "Simulated" => simulatedAdapter,
        "MicrosoftGraph" => microsoftGraphAdapter,
        _ => throw new InvalidOperationException("SharePoint provider mode is not supported.")
    };
}
