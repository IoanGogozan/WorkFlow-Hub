using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Api.Hardening;
using NorvixHub.Domain.Documents;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private static async Task<UploadReadResult> ReadUploadAsync(
        HttpRequest request,
        RequestLimitOptions requestLimits,
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

        if (!IsAllowedFile(file, requestLimits.Uploads, out var error))
        {
            return UploadReadResult.Invalid(error);
        }

        return UploadReadResult.Valid(file, form["title"].ToString());
    }

    private static bool IsAllowedFile(
        IFormFile file,
        UploadLimitOptions uploadLimits,
        out string error)
    {
        error = string.Empty;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedTypes = uploadLimits.AllowedFileTypes
            .Where(candidate => string.Equals(
                candidate.Extension,
                extension,
                StringComparison.OrdinalIgnoreCase))
            .Select(candidate => candidate.ContentType)
            .ToArray();
        if (allowedTypes.Length == 0)
        {
            error = "Unsupported file extension.";
            return false;
        }

        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            error = "File content type does not match the allowed list.";
            return false;
        }

        if (file.Length <= 0 || file.Length > uploadLimits.MaxFileBytes)
        {
            error = $"File size must be between 1 byte and {uploadLimits.MaxFileBytes} bytes.";
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
