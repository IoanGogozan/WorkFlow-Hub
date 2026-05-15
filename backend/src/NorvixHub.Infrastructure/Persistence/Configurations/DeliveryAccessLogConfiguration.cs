using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Delivery;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DeliveryAccessLogConfiguration : IEntityTypeConfiguration<DeliveryAccessLog>
{
    public void Configure(EntityTypeBuilder<DeliveryAccessLog> builder)
    {
        builder.ToTable("delivery_access_logs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Action).HasMaxLength(80).IsRequired();
        builder.Property(log => log.IpAddress).HasMaxLength(80);
        builder.Property(log => log.UserAgent).HasMaxLength(600);
        builder.HasIndex(log => new { log.TenantId, log.DeliveryPackageId, log.AccessedAt });
    }
}
