using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NorvixHub.Application.Organizations;

namespace NorvixHub.Infrastructure.Organizations;

public sealed class BrregClient(HttpClient httpClient, IOptions<BrregOptions> options) : IBrregClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<BrregOrganization>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var path = IsOrganizationNumber(query)
            ? $"enheter?organisasjonsnummer={Uri.EscapeDataString(query)}"
            : $"enheter?navn={Uri.EscapeDataString(query)}&navnMetodeForSoek=FORTLOEPENDE";
        using var document = await GetJsonAsync(path, cancellationToken);
        var embedded = document.RootElement.GetProperty("_embedded");
        var enheter = embedded.GetProperty("enheter");
        return enheter.EnumerateArray().Select(MapOrganization).ToList();
    }

    public async Task<BrregOrganization?> GetByOrganizationNumberAsync(
        string organizationNumber,
        CancellationToken cancellationToken)
    {
        EnsureBaseAddress();
        using var response = await httpClient.GetAsync($"enheter/{organizationNumber}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(content);
        return MapOrganization(document.RootElement);
    }

    private async Task<JsonDocument> GetJsonAsync(string path, CancellationToken cancellationToken)
    {
        EnsureBaseAddress();
        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(content);
    }

    private void EnsureBaseAddress()
    {
        httpClient.BaseAddress ??= new Uri(options.Value.BaseUrl);
    }

    private static BrregOrganization MapOrganization(JsonElement element)
    {
        return new BrregOrganization(
            ReadString(element, "organisasjonsnummer") ?? string.Empty,
            ReadString(element, "navn") ?? string.Empty,
            ReadNestedString(element, "organisasjonsform", "kode"),
            ReadNestedString(element, "forretningsadresse", "kommune"),
            ReadAddress(element),
            ReadBool(element, "erSlettet"),
            DateTimeOffset.UtcNow,
            element.GetRawText());
    }

    private static string? ReadAddress(JsonElement element)
    {
        if (!element.TryGetProperty("forretningsadresse", out var address))
        {
            return null;
        }

        var lines = new List<string>();
        if (address.TryGetProperty("adresse", out var addressLines))
        {
            lines.AddRange(addressLines.EnumerateArray().Select(line => line.GetString()).OfType<string>());
        }

        lines.AddRange(new[] { ReadString(address, "postnummer"), ReadString(address, "poststed") }.OfType<string>());
        return lines.Count == 0 ? null : string.Join(", ", lines);
    }

    private static string? ReadNestedString(JsonElement element, string objectName, string propertyName)
    {
        return element.TryGetProperty(objectName, out var nested) ? ReadString(nested, propertyName) : null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) ? value.GetString() : null;
    }

    private static bool ReadBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.GetBoolean();
    }

    private static bool IsOrganizationNumber(string value)
    {
        return value.Length == 9 && value.All(char.IsDigit);
    }
}
