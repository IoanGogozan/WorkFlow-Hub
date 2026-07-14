using Microsoft.EntityFrameworkCore;

namespace NorvixHub.ErpDemoReceiver.Persistence;

public sealed class ErpDemoReceiverDbContext(DbContextOptions<ErpDemoReceiverDbContext> options)
    : DbContext(options)
{
    public DbSet<ErpDemoReceipt> Receipts => Set<ErpDemoReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var receipt = modelBuilder.Entity<ErpDemoReceipt>();
        receipt.ToTable("erp_demo_receipts");
        receipt.HasKey(value => value.Id);
        receipt.HasIndex(value => value.IdempotencyKey).IsUnique();
        receipt.Property(value => value.ExternalReceiptId).HasMaxLength(80);
        receipt.Property(value => value.IdempotencyKey).HasMaxLength(160).IsRequired();
        receipt.Property(value => value.PayloadHash).HasMaxLength(64).IsRequired();
        receipt.Property(value => value.CustomerReference).HasMaxLength(120).IsRequired();
        receipt.Property(value => value.CaseNumber).HasMaxLength(80).IsRequired();
        receipt.Property(value => value.DocumentReference).HasMaxLength(160).IsRequired();
    }
}
