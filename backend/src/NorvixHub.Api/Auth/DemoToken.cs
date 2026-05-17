using System.Security.Cryptography;
using System.Text;

namespace NorvixHub.Api.Auth;

public static class DemoToken
{
    public static string Create()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    public static string? HashOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Hash(value);
    }
}
