using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Reviews;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class ReviewTaskConfiguration : IEntityTypeConfiguration<ReviewTask>
{
    public void Configure(EntityTypeBuilder<ReviewTask> builder)
    {
        builder.ToTable("review_tasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.EntityType).HasMaxLength(120).IsRequired();
        builder.Property(task => task.ReviewType).HasMaxLength(80).IsRequired();
        builder.Property(task => task.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(task => task.DecisionJson).HasColumnType("jsonb");
        builder.HasIndex(task => new { task.TenantId, task.Status });
        builder.HasIndex(task => new { task.TenantId, task.EntityType, task.EntityId });
    }
}

