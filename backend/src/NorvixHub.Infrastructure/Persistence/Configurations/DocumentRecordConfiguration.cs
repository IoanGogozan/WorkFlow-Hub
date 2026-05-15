using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Documents;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class DocumentRecordConfiguration : IEntityTypeConfiguration<DocumentRecord>
{
    public void Configure(EntityTypeBuilder<DocumentRecord> builder)
    {
        builder.ToTable("documents");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.Title).HasMaxLength(240).IsRequired();
        builder.Property(document => document.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(document => document.DocumentType).HasMaxLength(120);
        builder.HasIndex(document => new { document.TenantId, document.Status });
        builder.HasIndex(document => new { document.TenantId, document.CaseId });
    }
}

