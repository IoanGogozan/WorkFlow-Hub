using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.LiveDemo;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class LiveDemoRunStepConfiguration : IEntityTypeConfiguration<LiveDemoRunStep>
{
    public void Configure(EntityTypeBuilder<LiveDemoRunStep> builder)
    {
        builder.ToTable("live_demo_run_steps");
        builder.HasKey(step => step.Id);
        builder.Property(step => step.Key).HasMaxLength(80).IsRequired();
        builder.Property(step => step.PublicStage).HasMaxLength(40).IsRequired();
        builder.Property(step => step.Provider).HasMaxLength(120).IsRequired();
        builder.Property(step => step.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(step => step.EvidenceMode).HasMaxLength(40).IsRequired();
        builder.Property(step => step.PublicSummary).HasMaxLength(600);
        builder.Property(step => step.PublicEvidenceReference).HasMaxLength(256);
        builder.Property(step => step.PublicErrorCode).HasMaxLength(80);
        builder.Property(step => step.PublicErrorMessage).HasMaxLength(600);
        builder.HasIndex(step => new { step.TenantId, step.RunId, step.Key }).IsUnique();
        builder.HasIndex(step => new { step.RunId, step.Sequence });
    }
}
