using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Documents;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> AllowedContentTypes = new()
    {
        [".pdf"] = new[] { "application/pdf" },
        [".docx"] = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        [".xlsx"] = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        [".png"] = new[] { "image/png" },
        [".jpg"] = new[] { "image/jpeg" },
        [".jpeg"] = new[] { "image/jpeg" }
    };

    private static async Task<UploadReadResult> ReadUploadAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return UploadReadResult.Invalid("Multipart form content is required.");
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        if (file is null)
        {
            return UploadReadResult.Invalid("File field is required.");
        }

        if (!IsAllowedFile(file, out var error))
        {
            return UploadReadResult.Invalid(error);
        }

        return UploadReadResult.Valid(file, form["title"].ToString());
    }

    private static bool IsAllowedFile(IFormFile file, out string error)
    {
        error = string.Empty;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedContentTypes.TryGetValue(extension, out var allowedTypes))
        {
            error = "Unsupported file extension.";
            return false;
        }

        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            error = "File content type does not match the allowed list.";
            return false;
        }

        if (file.Length <= 0 || file.Length > MaxFileSizeBytes)
        {
            error = "File size must be between 1 byte and 10 MB.";
            return false;
        }

        return true;
    }

    private static DocumentRecord CreateDocument(UploadReadResult upload, ITenantContext tenantContext)
    {
        return new DocumentRecord
        {
            TenantId = tenantContext.TenantId!.Value,
            CreatedBy = tenantContext.UserId,
            Title = string.IsNullOrWhiteSpace(upload.Title) ? Path.GetFileName(upload.File!.FileName) : upload.Title.Trim()
        };
    }

    private static DocumentVersion CreateVersion(
        DocumentRecord document,
        StoredFile stored,
        IFormFile file,
        int versionNumber,
        ITenantContext tenantContext)
    {
        return new DocumentVersion
        {
            TenantId = document.TenantId,
            CreatedBy = tenantContext.UserId,
            DocumentId = document.Id,
            VersionNumber = versionNumber,
            BlobContainer = stored.Container,
            BlobName = stored.BlobName,
            OriginalFilename = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            SizeBytes = stored.SizeBytes,
            Sha256Hash = stored.Sha256Hash,
            UploadedByUserId = tenantContext.UserId
        };
    }

    private sealed record UploadReadResult(bool IsValid, IFormFile? File, string? Title, string Error)
    {
        public static UploadReadResult Valid(IFormFile file, string? title) => new(true, file, title, string.Empty);
        public static UploadReadResult Invalid(string error) => new(false, null, null, error);
    }
}
