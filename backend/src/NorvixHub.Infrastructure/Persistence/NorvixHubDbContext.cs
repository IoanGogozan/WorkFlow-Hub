using Microsoft.EntityFrameworkCore;
using NorvixHub.Domain.AI;
using NorvixHub.Domain.Audit;
using NorvixHub.Domain.Cases;
using NorvixHub.Domain.Customers;
using NorvixHub.Domain.Demo;
using NorvixHub.Domain.Delivery;
using NorvixHub.Domain.Documents;
using NorvixHub.Domain.Integrations;
using NorvixHub.Domain.Intake;
using NorvixHub.Domain.LiveDemo;
using NorvixHub.Domain.Reviews;
using NorvixHub.Domain.Tenants;
using NorvixHub.Domain.Users;

namespace NorvixHub.Infrastructure.Persistence;

public sealed class NorvixHubDbContext(DbContextOptions<NorvixHubDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserProfile> Users => Set<UserProfile>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<DemoSession> DemoSessions => Set<DemoSession>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<IntakeItem> IntakeItems => Set<IntakeItem>();
    public DbSet<IntakeAttachment> IntakeAttachments => Set<IntakeAttachment>();
    public DbSet<AiAnalysisRun> AiAnalysisRuns => Set<AiAnalysisRun>();
    public DbSet<ReviewTask> ReviewTasks => Set<ReviewTask>();
    public DbSet<CaseWorkspace> Cases => Set<CaseWorkspace>();
    public DbSet<CaseTask> CaseTasks => Set<CaseTask>();
    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<DocumentRecord> Documents => Set<DocumentRecord>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<DocumentLink> DocumentLinks => Set<DocumentLink>();
    public DbSet<IntegrationConnection> IntegrationConnections => Set<IntegrationConnection>();
    public DbSet<IntegrationSyncRun> IntegrationSyncRuns => Set<IntegrationSyncRun>();
    public DbSet<DeliveryPackage> DeliveryPackages => Set<DeliveryPackage>();
    public DbSet<DeliveryPackageItem> DeliveryPackageItems => Set<DeliveryPackageItem>();
    public DbSet<DeliveryLink> DeliveryLinks => Set<DeliveryLink>();
    public DbSet<DeliveryAccessLog> DeliveryAccessLogs => Set<DeliveryAccessLog>();
    public DbSet<LiveDemoRun> LiveDemoRuns => Set<LiveDemoRun>();
    public DbSet<LiveDemoRunStep> LiveDemoRunSteps => Set<LiveDemoRunStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NorvixHubDbContext).Assembly);
    }
}
