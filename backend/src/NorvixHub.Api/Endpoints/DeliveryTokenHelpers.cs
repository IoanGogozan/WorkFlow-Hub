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

    private static byte[] CreatePdfSummaryBytes(string title, string caseTitle, IReadOnlyCollection<string> documentTitles)
    {
        var lines = new[]
        {
            "%PDF-1.4",
            "% Norvix WorkFlow Hub delivery summary",
            $"Title: {title}",
            $"Case: {caseTitle}",
            $"Documents: {string.Join(", ", documentTitles)}",
            "%%EOF"
        };
        return Encoding.UTF8.GetBytes(string.Join('\n', lines));
    }
}
