using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace NorvixHub.ErpDemoReceiver.Persistence;

public static class ErpDemoPersistenceExtensions
{
    private const string DefaultDatabasePath = "data/erp-demo-receiver.db";

    public static IServiceCollection AddErpDemoPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ErpDemoReceiver")
            ?? $"Data Source={DefaultDatabasePath}";

        EnsureDatabaseDirectoryExists(connectionString);

        services.AddDbContext<ErpDemoReceiverDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }

    public static async Task InitializeErpDemoPersistenceAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDemoReceiverDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private static void EnsureDatabaseDirectoryExists(string connectionString)
    {
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }
    }
}
