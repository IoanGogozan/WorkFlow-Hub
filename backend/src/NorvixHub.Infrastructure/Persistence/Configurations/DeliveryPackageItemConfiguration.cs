using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Delivery;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DeliveryPackageItemConfiguration : IEntityTypeConfiguration<DeliveryPackageItem>
{
    public void Configure(EntityTypeBuilder<DeliveryPackageItem> builder)
    {
        builder.ToTable("delivery_package_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.DisplayName).HasMaxLength(240).IsRequired();
        builder.HasIndex(item => new { item.TenantId, item.DeliveryPackageId, item.DocumentId }).IsUnique();
    }
}
