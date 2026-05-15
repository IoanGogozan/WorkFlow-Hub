using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Documents;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("document_versions");
        builder.HasKey(version => version.Id);
        builder.Property(version => version.BlobContainer).HasMaxLength(120).IsRequired();
        builder.Property(version => version.BlobName).HasMaxLength(260).IsRequired();
        builder.Property(version => version.OriginalFilename).HasMaxLength(260).IsRequired();
        builder.Property(version => version.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(version => version.Sha256Hash).HasMaxLength(128).IsRequired();
        builder.HasIndex(version => new { version.TenantId, version.DocumentId, version.VersionNumber }).IsUnique();
    }
}

