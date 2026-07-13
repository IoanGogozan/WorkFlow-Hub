using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NorvixHub.ErpDemoReceiver.Persistence;
using Xunit;

namespace NorvixHub.ErpDemoReceiver.Tests.Persistence;

public sealed class ErpDemoReceiptPersistenceTests
{
    [Fact]
    public async Task Receipt_can_be_inserted_and_retrieved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync();
        var receipt = CreateReceipt("run-insert");
        receipt.RegisterAttempt();
        receipt.MarkReceived("ERP-DEMO-INSERT", DateTime.UtcNow);

        database.Context.Receipts.Add(receipt);
        await database.Context.SaveChangesAsync(cancellationToken);

        var stored = await database.Context.Receipts.SingleAsync(cancellationToken);
        stored.IdempotencyKey.Should().Be("run-insert");
        stored.ExternalReceiptId.Should().Be("ERP-DEMO-INSERT");
        stored.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task Duplicate_idempotency_key_is_rejected()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Receipts.AddRange(CreateReceipt("run-duplicate"), CreateReceipt("run-duplicate"));

        var action = () => database.Context.SaveChangesAsync(cancellationToken);

        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Receipt_persists_when_database_context_is_restarted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var path = Path.Combine(Path.GetTempPath(), $"norvix-erp-{Guid.NewGuid():N}.db");
        try
        {
            await using (var first = await TestDatabase.CreateAsync(path))
            {
                var receipt = CreateReceipt("run-restart");
                receipt.RegisterAttempt();
                receipt.MarkFailOnceTriggered();
                first.Context.Receipts.Add(receipt);
                await first.Context.SaveChangesAsync(cancellationToken);
            }

            await using var restarted = await TestDatabase.CreateAsync(path);
            var stored = await restarted.Context.Receipts.SingleAsync(cancellationToken);
            stored.IdempotencyKey.Should().Be("run-restart");
            stored.AttemptCount.Should().Be(1);
            stored.FailOnceTriggered.Should().BeTrue();
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static ErpDemoReceipt CreateReceipt(string idempotencyKey) => new(
        Guid.NewGuid(),
        idempotencyKey,
        new string('a', 64),
        "FICTIONAL-CUSTOMER-001",
        "LIVE-2026-TEST",
        "demo-document.pdf");

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly string path;
        private readonly bool ownsPath;

        private TestDatabase(ErpDemoReceiverDbContext context, string path, bool ownsPath)
        {
            Context = context;
            this.path = path;
            this.ownsPath = ownsPath;
        }

        public ErpDemoReceiverDbContext Context { get; }

        public static async Task<TestDatabase> CreateAsync(string? existingPath = null)
        {
            var ownsPath = existingPath is null;
            var path = existingPath ?? Path.Combine(Path.GetTempPath(), $"norvix-erp-{Guid.NewGuid():N}.db");
            var options = new DbContextOptionsBuilder<ErpDemoReceiverDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False")
                .Options;
            var context = new ErpDemoReceiverDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return new TestDatabase(context, path, ownsPath);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            if (ownsPath)
            {
                File.Delete(path);
            }
        }
    }
}
