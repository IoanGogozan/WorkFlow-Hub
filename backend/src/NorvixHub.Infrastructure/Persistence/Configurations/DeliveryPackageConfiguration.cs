using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Delivery;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DeliveryPackageConfiguration : IEntityTypeConfiguration<DeliveryPackage>
{
    public void Configure(EntityTypeBuilder<DeliveryPackage> builder)
    {
        builder.ToTable("delivery_packages");
        builder.HasKey(package => package.Id);
        builder.Property(package => package.Title).HasMaxLength(240).IsRequired();
        builder.Property(package => package.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(package => new { package.TenantId, package.CaseId });
    }
}
