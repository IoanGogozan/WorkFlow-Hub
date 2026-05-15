using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Cases;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class CaseTaskConfiguration : IEntityTypeConfiguration<CaseTask>
{
    public void Configure(EntityTypeBuilder<CaseTask> builder)
    {
        builder.ToTable("case_tasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Title).HasMaxLength(240).IsRequired();
        builder.Property(task => task.Description).HasMaxLength(4000);
        builder.Property(task => task.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(task => new { task.TenantId, task.CaseId });
        builder.HasIndex(task => new { task.TenantId, task.Status });
    }
}

