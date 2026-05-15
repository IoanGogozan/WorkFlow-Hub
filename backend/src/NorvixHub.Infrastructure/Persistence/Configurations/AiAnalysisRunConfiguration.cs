using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.AI;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class AiAnalysisRunConfiguration : IEntityTypeConfiguration<AiAnalysisRun>
{
    public void Configure(EntityTypeBuilder<AiAnalysisRun> builder)
    {
        builder.ToTable("ai_analysis_runs");
        builder.HasKey(run => run.Id);
        builder.Property(run => run.EntityType).HasMaxLength(120).IsRequired();
        builder.Property(run => run.Provider).HasMaxLength(80).IsRequired();
        builder.Property(run => run.Model).HasMaxLength(120).IsRequired();
        builder.Property(run => run.PromptVersion).HasMaxLength(120).IsRequired();
        builder.Property(run => run.InputHash).HasMaxLength(128).IsRequired();
        builder.Property(run => run.OutputJson).HasColumnType("jsonb").IsRequired();
        builder.Property(run => run.Confidence).HasPrecision(5, 4);
        builder.Property(run => run.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(run => new { run.TenantId, run.EntityType, run.EntityId });
    }
}

