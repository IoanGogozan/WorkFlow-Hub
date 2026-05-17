using System.Text;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Documents;
using NorvixHub.Application.Tenancy;
using NorvixHub.Domain.Documents;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    private const string SampleDocumentTitle = "Demo inspection report";
    private const string SampleDocumentFilename = "demo-inspection-report.pdf";

    private static async Task<IResult> CreateSampleDocument(
        ITenantContext tenantContext,
        NorvixHubDbContext dbContext,
        IFileStorage fileStorage,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanWriteDocuments(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        await using var content = new MemoryStream(CreateSamplePdfBytes());
        var stored = await fileStorage.SaveAsync(
            content,
            SampleDocumentFilename,
            "application/pdf",
            cancellationToken);

        var document = new DocumentRecord
        {
            TenantId = tenantContext.TenantId!.Value,
            CreatedBy = tenantContext.UserId,
            Title = SampleDocumentTitle
        };
        var version = new DocumentVersion
        {
            TenantId = document.TenantId,
            CreatedBy = tenantContext.UserId,
            DocumentId = document.Id,
            VersionNumber = 1,
            BlobContainer = stored.Container,
            BlobName = stored.BlobName,
            OriginalFilename = SampleDocumentFilename,
            ContentType = "application/pdf",
            SizeBytes = stored.SizeBytes,
            Sha256Hash = stored.Sha256Hash,
            UploadedByUserId = tenantContext.UserId
        };
        document.SetCurrentVersion(version.Id, tenantContext.UserId, DateTimeOffset.UtcNow);

        dbContext.Documents.Add(document);
        dbContext.DocumentVersions.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            auditEventWriter,
            document,
            tenantContext,
            httpContext,
            "SampleDocumentCreated",
            cancellationToken);

        return Results.Created($"/api/documents/{document.Id}", ToResponse(document));
    }

    private static byte[] CreateSamplePdfBytes()
    {
        var lines = new[]
        {
            "BT",
            "/F1 18 Tf",
            "72 760 Td",
            "(Norvix WorkFlow Hub - Demo Inspection Report) Tj",
            "/F1 11 Tf",
            "0 -28 Td",
            "(This is fictional sample documentation for the public demo.) Tj",
            "0 -18 Td",
            "(It is safe to classify, approve, link to a case, and include in a delivery package.) Tj",
            "0 -18 Td",
            "(Do not upload personal or confidential files to the public demo.) Tj",
            "ET"
        };
        var content = string.Join('\n', lines);
        var contentLength = Encoding.ASCII.GetByteCount(content);
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {contentLength} >>\nstream\n{content}\nendstream"
        };

        var builder = new StringBuilder();
        var offsets = new List<int> { 0 };
        builder.Append("%PDF-1.4\n");
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(index + 1).Append(" 0 obj\n");
            builder.Append(objects[index]).Append('\n');
            builder.Append("endobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n");
        builder.Append("0 ").Append(objects.Length + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(offset.ToString("D10")).Append(" 00000 n \n");
        }

        builder.Append("trailer\n");
        builder.Append("<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\n");
        builder.Append("startxref\n");
        builder.Append(xrefOffset).Append('\n');
        builder.Append("%%EOF");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }
}
