namespace NorvixHub.Application.Documents;

public sealed record StoredFile(
    string Container,
    string BlobName,
    string Sha256Hash,
    long SizeBytes);

