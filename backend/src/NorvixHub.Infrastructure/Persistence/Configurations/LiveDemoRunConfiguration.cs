using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.LiveDemo;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class LiveDemoRunConfiguration : IEntityTypeConfiguration<LiveDemoRun>
{
    public void Configure(EntityTypeBuilder<LiveDemoRun> builder)
    {
        builder.ToTable("live_demo_runs");
        builder.HasKey(run => run.Id);
        builder.Property(run => run.ScenarioKey).HasMaxLength(80).IsRequired();
        builder.Property(run => run.CorrelationId).HasMaxLength(128).IsRequired();
        builder.Property(run => run.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(run => run.CurrentStepKey).HasMaxLength(80);
        builder.Property(run => run.OrganizationNumber).HasMaxLength(20).IsRequired();
        builder.Property(run => run.CustomerReference).HasMaxLength(120).IsRequired();
        builder.Property(run => run.RequestTitle).HasMaxLength(240).IsRequired();
        builder.Property(run => run.RequestBody).HasMaxLength(4000).IsRequired();
        builder.Property(run => run.PublicErrorCode).HasMaxLength(80);
        builder.Property(run => run.PublicErrorMessage).HasMaxLength(600);
        builder.Property(run => run.BrregMode).HasMaxLength(20);
        builder.Property(run => run.SharePointDriveId).HasMaxLength(256);
        builder.Property(run => run.SharePointFolderItemId).HasMaxLength(256);
        builder.Property(run => run.SharePointFileItemId).HasMaxLength(256);
        builder.Property(run => run.ErpReceiptId).HasMaxLength(256);
        builder.HasIndex(run => new { run.TenantId, run.CreatedAt });
        builder.HasIndex(run => new { run.TenantId, run.Status, run.CreatedAt });
        builder.HasIndex(run => new { run.TenantId, run.DemoSessionId });
    }
}
