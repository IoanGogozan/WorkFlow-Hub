namespace NorvixHub.Infrastructure.Documents;

public sealed class LocalFileStorageOptions
{
    public string RootPath { get; init; } = "storage/documents";
    public string Container { get; init; } = "documents";
}

