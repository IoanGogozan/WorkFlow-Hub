namespace NorvixHub.Application.Documents;

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(
        Stream content,
        string originalFilename,
        string contentType,
        CancellationToken cancellationToken);
}

