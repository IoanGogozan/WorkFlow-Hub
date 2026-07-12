using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.SharePoint;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class SimulatedSharePointDocumentItemConfiguration : IEntityTypeConfiguration<SimulatedSharePointDocumentItem>
{
    public void Configure(EntityTypeBuilder<SimulatedSharePointDocumentItem> builder)
    {
        builder.ToTable("simulated_sharepoint_document_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.SiteId).HasMaxLength(256).IsRequired();
        builder.Property(item => item.DriveId).HasMaxLength(256).IsRequired();
        builder.Property(item => item.ExternalItemId).HasMaxLength(256).IsRequired();
        builder.Property(item => item.ParentPath).HasMaxLength(1024).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(512).IsRequired();
        builder.Property(item => item.ETag).HasMaxLength(256).IsRequired();
        builder.Property(item => item.Version).HasMaxLength(40).IsRequired();
        builder.Property(item => item.MetadataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.SyncStatus).HasMaxLength(40).IsRequired();
        builder.Property(item => item.IdempotencyKey).HasMaxLength(256).IsRequired();
        builder.HasIndex(item => new { item.TenantId, item.DocumentId }).IsUnique();
        builder.HasIndex(item => new { item.TenantId, item.IdempotencyKey }).IsUnique();
        builder.HasIndex(item => new { item.TenantId, item.CaseId, item.LastSyncedAt });
    }
}
