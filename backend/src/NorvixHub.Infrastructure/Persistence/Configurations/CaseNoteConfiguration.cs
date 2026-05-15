using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Cases;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class CaseNoteConfiguration : IEntityTypeConfiguration<CaseNote>
{
    public void Configure(EntityTypeBuilder<CaseNote> builder)
    {
        builder.ToTable("case_notes");
        builder.HasKey(note => note.Id);
        builder.Property(note => note.Body).HasMaxLength(4000).IsRequired();
        builder.Property(note => note.Visibility).HasMaxLength(40).IsRequired();
        builder.HasIndex(note => new { note.TenantId, note.CaseId });
    }
}

