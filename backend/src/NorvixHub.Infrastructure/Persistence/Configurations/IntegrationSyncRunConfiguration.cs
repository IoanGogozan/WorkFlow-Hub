using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Integrations;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class IntegrationSyncRunConfiguration : IEntityTypeConfiguration<IntegrationSyncRun>
{
    public void Configure(EntityTypeBuilder<IntegrationSyncRun> builder)
    {
        builder.ToTable("integration_sync_runs");
        builder.HasKey(run => run.Id);
        builder.Property(run => run.Provider).HasMaxLength(80).IsRequired();
        builder.Property(run => run.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(run => run.TriggeredBy).HasMaxLength(40).IsRequired();
        builder.Property(run => run.ErrorMessage).HasMaxLength(600);
        builder.HasIndex(run => new { run.TenantId, run.Provider, run.StartedAt });
        builder.HasIndex(run => new { run.TenantId, run.ConnectionId });
    }
}
