using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Documents;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DocumentLinkConfiguration : IEntityTypeConfiguration<DocumentLink>
{
    public void Configure(EntityTypeBuilder<DocumentLink> builder)
    {
        builder.ToTable("document_links");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.EntityType).HasMaxLength(120).IsRequired();
        builder.HasIndex(link => new { link.TenantId, link.DocumentId, link.EntityType, link.EntityId }).IsUnique();
    }
}

