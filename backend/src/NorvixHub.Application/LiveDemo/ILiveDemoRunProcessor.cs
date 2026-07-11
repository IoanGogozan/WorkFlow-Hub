namespace NorvixHub.Application.LiveDemo;

public interface ILiveDemoRunProcessor
{
    Task ProcessAsync(Guid runId, CancellationToken cancellationToken);
}
