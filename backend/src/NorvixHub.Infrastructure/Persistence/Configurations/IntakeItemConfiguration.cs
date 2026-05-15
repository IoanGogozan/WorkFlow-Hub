using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Intake;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class IntakeItemConfiguration : IEntityTypeConfiguration<IntakeItem>
{
    public void Configure(EntityTypeBuilder<IntakeItem> builder)
    {
        builder.ToTable("intake_items");
        builder.HasKey(intake => intake.Id);
        builder.Property(intake => intake.Source).HasConversion<string>().HasMaxLength(40);
        builder.Property(intake => intake.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(intake => intake.Subject).HasMaxLength(240).IsRequired();
        builder.Property(intake => intake.Body).HasMaxLength(8000).IsRequired();
        builder.Property(intake => intake.CustomerName).HasMaxLength(240);
        builder.Property(intake => intake.OrganizationNumber).HasMaxLength(20);
        builder.Property(intake => intake.Category).HasMaxLength(120);
        builder.Property(intake => intake.Urgency).HasMaxLength(40);
        builder.HasIndex(intake => new { intake.TenantId, intake.Status });
        builder.HasIndex(intake => new { intake.TenantId, intake.CreatedAt });
    }
}

