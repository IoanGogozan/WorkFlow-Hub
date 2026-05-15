using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorvixHub.Domain.Cases;

namespace NorvixHub.Infrastructure.Persistence.Configurations;

public sealed class CaseWorkspaceConfiguration : IEntityTypeConfiguration<CaseWorkspace>
{
    public void Configure(EntityTypeBuilder<CaseWorkspace> builder)
    {
        builder.ToTable("cases");
        builder.HasKey(caseWorkspace => caseWorkspace.Id);
        builder.Property(caseWorkspace => caseWorkspace.CaseNumber).HasMaxLength(40).IsRequired();
        builder.Property(caseWorkspace => caseWorkspace.Title).HasMaxLength(240).IsRequired();
        builder.Property(caseWorkspace => caseWorkspace.Description).HasMaxLength(8000);
        builder.Property(caseWorkspace => caseWorkspace.Status).HasConversion<string>().HasMaxLength(60);
        builder.Property(caseWorkspace => caseWorkspace.MissingInformationJson).HasColumnType("jsonb");
        builder.Property(caseWorkspace => caseWorkspace.ExternalProjectId).HasMaxLength(120);
        builder.HasIndex(caseWorkspace => new { caseWorkspace.TenantId, caseWorkspace.CaseNumber }).IsUnique();
        builder.HasIndex(caseWorkspace => new { caseWorkspace.TenantId, caseWorkspace.Status });
    }
}

