using Microsoft.EntityFrameworkCore;
using NorvixHub.Domain.AI;
using NorvixHub.Domain.Audit;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.Reviews;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;

namespace NorvixHub.Infrastructure.Persistence;

public sealed class NorvixHubDbContext(DbContextOptions<NorvixHubDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserProfile> Users => Set<UserProfile>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<IntakeItem> IntakeItems => Set<IntakeItem>();
    public DbSet<IntakeAttachment> IntakeAttachments => Set<IntakeAttachment>();
    public DbSet<AiAnalysisRun> AiAnalysisRuns => Set<AiAnalysisRun>();
    public DbSet<ReviewTask> ReviewTasks => Set<ReviewTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NorvixHubDbContext).Assembly);
    }
}
