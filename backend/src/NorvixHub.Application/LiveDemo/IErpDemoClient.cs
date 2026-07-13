namespace NorvixHub.Application.LiveDemo;

public interface IErpDemoClient
{
    Task<ErpDemoResult> SendAsync(ErpDemoRequest request, CancellationToken cancellationToken);
}
