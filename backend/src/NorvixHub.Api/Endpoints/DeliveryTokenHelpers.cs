using System.Security.Cryptography;
using System.Text;

namespace NorvixHub.Api.Endpoints;

public static partial class DeliveryEndpoints
{
    private static string CreateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private static byte[] CreatePdfSummaryBytes(
        string title,
        string caseTitle,
        string caseNumber,
        string? customerName,
        Guid? deliveryLinkId,
        IReadOnlyCollection<string> documentTitles)
    {
        var contentLines = new List<string>
        {
            "BT",
            "/F1 18 Tf",
            "72 760 Td",
            $"({EscapePdfText(title)}) Tj",
            "/F1 11 Tf",
            "0 -28 Td",
            $"(Case: {EscapePdfText(caseNumber)} - {EscapePdfText(caseTitle)}) Tj",
            "0 -18 Td",
            $"(Customer: {EscapePdfText(customerName ?? "Not set")}) Tj",
            "0 -18 Td",
            $"(Delivery link ID: {EscapePdfText(deliveryLinkId?.ToString() ?? "Not created yet")}) Tj",
            "0 -18 Td",
            $"(Generated: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} UTC) Tj",
            "0 -28 Td",
            "(Documents:) Tj"
        };

        foreach (var documentTitle in documentTitles)
        {
            contentLines.Add("0 -16 Td");
            contentLines.Add($"(- {EscapePdfText(documentTitle)}) Tj");
        }

        contentLines.Add("0 -36 Td");
        contentLines.Add("(Norvix WorkFlow Hub) Tj");
        contentLines.Add("ET");

        var content = string.Join('\n', contentLines);
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

    private static string EscapePdfText(string value)
    {
        return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }

    private static string SanitizeFilename(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(character => invalid.Contains(character) ? '-' : character).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "delivery-package" : sanitized.Trim();
    }
}
