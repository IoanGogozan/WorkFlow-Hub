using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Users;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>
{
    public void Configure(EntityTypeBuilder<TenantMembership> builder)
    {
        builder.ToTable("tenant_memberships");
        builder.HasKey(membership => membership.Id);
        builder.Property(membership => membership.Role).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(membership => new { membership.TenantId, membership.UserId }).IsUnique();
    }
}

