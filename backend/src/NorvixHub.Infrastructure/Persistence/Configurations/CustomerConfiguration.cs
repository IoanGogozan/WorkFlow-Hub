using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Customers;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Name).HasMaxLength(240).IsRequired();
        builder.Property(customer => customer.OrganizationNumber).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.BrregDataJson).HasColumnType("jsonb");
        builder.Property(customer => customer.Source).HasMaxLength(80).IsRequired();
        builder.Property(customer => customer.PrimaryContactName).HasMaxLength(200);
        builder.Property(customer => customer.PrimaryContactEmail).HasMaxLength(320);
        builder.HasIndex(customer => new { customer.TenantId, customer.OrganizationNumber }).IsUnique();
    }
}

