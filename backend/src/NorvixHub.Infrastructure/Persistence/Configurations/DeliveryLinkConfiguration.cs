using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Delivery;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DeliveryLinkConfiguration : IEntityTypeConfiguration<DeliveryLink>
{
    public void Configure(EntityTypeBuilder<DeliveryLink> builder)
    {
        builder.ToTable("delivery_links");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(link => link.RecipientEmail).HasMaxLength(320);
        builder.HasIndex(link => link.TokenHash).IsUnique();
        builder.HasIndex(link => new { link.TenantId, link.DeliveryPackageId });
    }
}
