namespace NorvixHub.Infrastructure.Documents;

public sealed class AzureBlobFileStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Container { get; set; } = "documents";
}
