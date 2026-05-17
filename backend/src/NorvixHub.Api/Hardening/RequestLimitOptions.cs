namespace NorvixHub.Api.Hardening;

public sealed class RequestLimitOptions
{
    public long MaxRequestBodyBytes { get; set; } = 6 * 1024 * 1024;
    public UploadLimitOptions Uploads { get; set; } = new();
}

public sealed class UploadLimitOptions
{
    public long MaxFileBytes { get; set; } = 5 * 1024 * 1024;
    public List<AllowedUploadFileType> AllowedFileTypes { get; set; } =
    [
        new() { Extension = ".pdf", ContentType = "application/pdf" },
        new() { Extension = ".png", ContentType = "image/png" },
        new() { Extension = ".jpg", ContentType = "image/jpeg" },
        new() { Extension = ".jpeg", ContentType = "image/jpeg" }
    ];
}

public sealed class AllowedUploadFileType
{
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
