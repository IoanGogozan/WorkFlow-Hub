using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using NorvixHub.Application.Documents;
using System.Security.Cryptography;

namespace NorvixHub.Infrastructure.Documents;

public sealed class AzureBlobFileStorage(IOptions<AzureBlobFileStorageOptions> options) : IFileStorage
{
    private readonly AzureBlobFileStorageOptions options = options.Value;

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var container = CreateContainerClient();
        await container.CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken: cancellationToken);

        var extension = Path.GetExtension(fileName);
        var blobName = string.IsNullOrWhiteSpace(extension)
            ? Guid.NewGuid().ToString("N")
            : $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        await using var uploadContent = new MemoryStream();
        await content.CopyToAsync(uploadContent, cancellationToken);
        uploadContent.Position = 0;

        var hash = SHA256.HashData(uploadContent);
        uploadContent.Position = 0;

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(
            uploadContent,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            cancellationToken);

        return new StoredFile(
            options.Container,
            blobName,
            Convert.ToHexString(hash),
            uploadContent.Length);
    }

    public async Task<StoredFileContent?> OpenReadAsync(
        string container,
        string blobName,
        CancellationToken cancellationToken)
    {
        var blob = CreateContainerClient(container).GetBlobClient(blobName);
        if (!await blob.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return new StoredFileContent(
            response.Value.Content,
            response.Value.Details.ContentLength);
    }

    public async Task DeleteAsync(
        string container,
        string blobName,
        CancellationToken cancellationToken)
    {
        var blob = CreateContainerClient(container).GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private BlobContainerClient CreateContainerClient()
    {
        return CreateContainerClient(options.Container);
    }

    private BlobContainerClient CreateContainerClient(string container)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("Azure Blob storage requires Storage:AzureBlob:ConnectionString.");
        }

        if (string.IsNullOrWhiteSpace(container))
        {
            throw new InvalidOperationException("Azure Blob storage requires a container name.");
        }

        return new BlobContainerClient(options.ConnectionString, container);
    }
}
