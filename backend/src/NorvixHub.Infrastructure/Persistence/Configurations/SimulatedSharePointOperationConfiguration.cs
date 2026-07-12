using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.SharePoint;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class SimulatedSharePointOperationConfiguration : IEntityTypeConfiguration<SimulatedSharePointOperation>
{
    public void Configure(EntityTypeBuilder<SimulatedSharePointOperation> builder)
    {
        builder.ToTable("simulated_sharepoint_operations");
        builder.HasKey(operation => operation.Id);
        builder.Property(operation => operation.Operation).HasMaxLength(80).IsRequired();
        builder.Property(operation => operation.HttpMethod).HasMaxLength(12).IsRequired();
        builder.Property(operation => operation.Target).HasMaxLength(1024).IsRequired();
        builder.Property(operation => operation.RequestSummaryJson).HasColumnType("jsonb");
        builder.Property(operation => operation.ResponseSummaryJson).HasColumnType("jsonb");
        builder.Property(operation => operation.ErrorCode).HasMaxLength(80);
        builder.Property(operation => operation.ErrorMessage).HasMaxLength(600);
        builder.HasIndex(operation => new { operation.TenantId, operation.CreatedAt });
        builder.HasIndex(operation => new { operation.TenantId, operation.IntegrationSyncRunId });
        builder.HasIndex(operation => new { operation.TenantId, operation.LiveDemoRunId });
    }
}
