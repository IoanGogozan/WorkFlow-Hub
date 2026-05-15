using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Audit;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.HasKey(auditEvent => auditEvent.Id);
        builder.Property(auditEvent => auditEvent.ActorType).HasMaxLength(60).IsRequired();
        builder.Property(auditEvent => auditEvent.EntityType).HasMaxLength(120).IsRequired();
        builder.Property(auditEvent => auditEvent.EntityId).HasMaxLength(120).IsRequired();
        builder.Property(auditEvent => auditEvent.Action).HasMaxLength(120).IsRequired();
        builder.HasIndex(auditEvent => new { auditEvent.TenantId, auditEvent.CreatedAt });
    }
}

