using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using NorvixHub.Application.Documents;

namespace NorvixHub.Infrastructure.Documents;

public sealed class LocalFileStorage(IOptions<LocalFileStorageOptions> options) : IFileStorage
{
    public async Task<StoredFile> SaveAsync(
        Stream content,
        string originalFilename,
        string contentType,
        CancellationToken cancellationToken)
    {
        var blobName = $"{Guid.NewGuid():N}{Path.GetExtension(originalFilename).ToLowerInvariant()}";
        var rootPath = options.Value.RootPath;
        Directory.CreateDirectory(rootPath);

        var targetPath = Path.Combine(rootPath, blobName);
        await using var target = File.Create(targetPath);
        using var sha256 = SHA256.Create();
        await using var hashingStream = new CryptoStream(target, sha256, CryptoStreamMode.Write);
        await content.CopyToAsync(hashingStream, cancellationToken);
        await hashingStream.FlushAsync(cancellationToken);
        hashingStream.FlushFinalBlock();

        return new StoredFile(
            options.Value.Container,
            blobName,
            Convert.ToHexString(sha256.Hash ?? Array.Empty<byte>()),
            new FileInfo(targetPath).Length);
    }
}

