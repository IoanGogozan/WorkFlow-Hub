using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Integrations;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class IntegrationConnectionConfiguration : IEntityTypeConfiguration<IntegrationConnection>
{
    public void Configure(EntityTypeBuilder<IntegrationConnection> builder)
    {
        builder.ToTable("integration_connections");
        builder.HasKey(connection => connection.Id);
        builder.Property(connection => connection.Provider).HasMaxLength(80).IsRequired();
        builder.Property(connection => connection.DisplayName).HasMaxLength(160).IsRequired();
        builder.Property(connection => connection.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(connection => connection.SettingsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(connection => connection.LastError).HasMaxLength(600);
        builder.HasIndex(connection => new { connection.TenantId, connection.Provider }).IsUnique();
    }
}
