using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Intake;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class IntakeAttachmentConfiguration : IEntityTypeConfiguration<IntakeAttachment>
{
    public void Configure(EntityTypeBuilder<IntakeAttachment> builder)
    {
        builder.ToTable("intake_attachments");
        builder.HasKey(attachment => attachment.Id);
        builder.Property(attachment => attachment.OriginalFilename).HasMaxLength(260).IsRequired();
        builder.Property(attachment => attachment.ContentType).HasMaxLength(120).IsRequired();
        builder.HasIndex(attachment => new { attachment.TenantId, attachment.IntakeItemId });
    }
}

