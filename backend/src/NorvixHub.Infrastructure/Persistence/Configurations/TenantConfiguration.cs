using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Tenants;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(tenant => tenant.Id);
        builder.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
        builder.Property(tenant => tenant.Slug).HasMaxLength(120).IsRequired();
        builder.Property(tenant => tenant.OrganizationNumber).HasMaxLength(20);
        builder.Property(tenant => tenant.CountryCode).HasMaxLength(2).IsRequired();
        builder.HasIndex(tenant => tenant.Slug).IsUnique();
    }
}

