using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NorvixHub.Application.LiveDemo;

namespace NorvixHub.Infrastructure.LiveDemo;

public sealed class ErpDemoClient : IErpDemoClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient httpClient;
    private readonly ErpDemoOptions options;

    public ErpDemoClient(HttpClient httpClient, IOptions<ErpDemoOptions> configuredOptions)
    {
        this.httpClient = httpClient;
        options = configuredOptions.Value;
        httpClient.BaseAddress ??= new Uri(options.BaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));
    }

    public async Task<ErpDemoResult> SendAsync(
        ErpDemoRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.SigningSecret))
        {
            return new ErpDemoResult(ErpDemoResultStatus.InvalidConfiguration);
        }

        var rawBody = CreateCanonicalJson(request);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var signature = CreateSignature(options.SigningSecret, timestamp, rawBody);
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/demo-orders")
        {
            Content = new ByteArrayContent(rawBody)
        };
        message.Content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        message.Headers.Add("X-Norvix-Timestamp", timestamp);
        message.Headers.Add("X-Norvix-Signature", signature);
        message.Headers.Add("Idempotency-Key", $"live-demo-{request.RunId:N}");
        if (request.FailOnce)
        {
            message.Headers.Add("X-Demo-Fail-Once", "true");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ErpDemoResult(ErpDemoResultStatus.Timeout);
        }
        catch (HttpRequestException)
        {
            return new ErpDemoResult(ErpDemoResultStatus.Unavailable);
        }

        using (response)
        {
            if (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK)
            {
                return await ReadSuccessAsync(response, cancellationToken);
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new ErpDemoResult(ErpDemoResultStatus.Unauthorized),
                HttpStatusCode.Conflict => new ErpDemoResult(ErpDemoResultStatus.Conflict),
                HttpStatusCode.ServiceUnavailable => new ErpDemoResult(ErpDemoResultStatus.Unavailable),
                _ => new ErpDemoResult(ErpDemoResultStatus.InvalidResponse)
            };
        }
    }

    private static async Task<ErpDemoResult> ReadSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var receipt = await JsonSerializer.DeserializeAsync<ReceiptResponse>(
                stream,
                JsonOptions,
                cancellationToken);
            if (receipt is null ||
                !receipt.ReceiptId.StartsWith("ERP-DEMO-", StringComparison.Ordinal) ||
                !string.Equals(receipt.Status, "Received", StringComparison.Ordinal))
            {
                return new ErpDemoResult(ErpDemoResultStatus.InvalidResponse);
            }

            return new ErpDemoResult(
                ErpDemoResultStatus.Received,
                receipt.ReceiptId,
                receipt.Duplicate,
                receipt.ReceivedAt);
        }
        catch (JsonException)
        {
            return new ErpDemoResult(ErpDemoResultStatus.InvalidResponse);
        }
    }

    private static byte[] CreateCanonicalJson(ErpDemoRequest request)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("customerReference", request.CustomerReference);
            writer.WriteString("caseNumber", request.CaseNumber);
            writer.WriteString("documentReference", request.DocumentReference);
            writer.WriteString("runId", request.RunId);
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    private static string CreateSignature(string secret, string timestamp, byte[] rawBody)
    {
        var prefix = Encoding.UTF8.GetBytes($"{timestamp}.");
        var signedBytes = new byte[prefix.Length + rawBody.Length];
        Buffer.BlockCopy(prefix, 0, signedBytes, 0, prefix.Length);
        Buffer.BlockCopy(rawBody, 0, signedBytes, prefix.Length, rawBody.Length);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexStringLower(hmac.ComputeHash(signedBytes));
    }

    private sealed record ReceiptResponse(
        string ReceiptId,
        string Status,
        bool Duplicate,
        DateTime ReceivedAt);
}
