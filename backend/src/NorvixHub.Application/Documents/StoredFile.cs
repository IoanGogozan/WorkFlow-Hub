namespace NorvixHub.Application.Documents;

public sealed record StoredFile(
    string Container,
    string BlobName,
    string Sha256Hash,
    long SizeBytes);

public sealed record StoredFileContent(Stream Content, long SizeBytes);
