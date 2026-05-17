namespace NorvixHub.Application.Documents;

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(
        Stream content,
        string originalFilename,
        string contentType,
        CancellationToken cancellationToken);

    Task<StoredFileContent?> OpenReadAsync(
        string container,
        string blobName,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string container,
        string blobName,
        CancellationToken cancellationToken);
}
