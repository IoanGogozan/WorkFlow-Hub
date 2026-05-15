using NorvixHub.Application.Audit;
using NorvixHub.Domain.Audit;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Infrastructure.Audit;

public sealed class DatabaseAuditEventWriter(NorvixHubDbContext dbContext) : IAuditEventWriter
{
    public async Task WriteAsync(AuditEventRequest request, CancellationToken cancellationToken)
    {
        dbContext.AuditEvents.Add(new AuditEvent
        {
            TenantId = request.TenantId,
            ActorUserId = request.ActorUserId,
            ActorType = request.ActorType,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Action = request.Action,
            BeforeJson = request.BeforeJson,
            AfterJson = request.AfterJson,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            CorrelationId = request.CorrelationId
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

