namespace NorvixHub.Infrastructure.Persistence;

public sealed record DemoSessionCleanupResult(
    int SessionsDeleted,
    int TenantsDeleted,
    int UsersDeleted,
    int FilesDeleted,
    int FileDeleteFailures);
