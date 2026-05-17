using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Demo;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DemoSessionConfiguration : IEntityTypeConfiguration<DemoSession>
{
    public void Configure(EntityTypeBuilder<DemoSession> builder)
    {
        builder.ToTable("demo_sessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(session => session.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(session => session.IpHash).HasMaxLength(128);
        builder.Property(session => session.UserAgentHash).HasMaxLength(128);
        builder.HasIndex(session => session.TokenHash).IsUnique();
        builder.HasIndex(session => session.TenantId);
        builder.HasIndex(session => session.ExpiresAt);
        builder.HasIndex(session => new { session.Status, session.ExpiresAt });
    }
}
