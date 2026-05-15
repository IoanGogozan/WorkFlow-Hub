namespace NorvixHub.Application.Audit;

public interface IAuditEventWriter
{
    Task WriteAsync(AuditEventRequest request, CancellationToken cancellationToken);
}

