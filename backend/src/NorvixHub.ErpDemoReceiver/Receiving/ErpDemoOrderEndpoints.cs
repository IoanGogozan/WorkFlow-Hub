using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NorvixHub.ErpDemoReceiver.Persistence;

namespace NorvixHub.ErpDemoReceiver.Receiving;

public static class ErpDemoOrderEndpoints
{
    private const string TimestampHeader = "X-Norvix-Timestamp";
    private const string SignatureHeader = "X-Norvix-Signature";
    private const string IdempotencyHeader = "Idempotency-Key";
    private const string FailOnceHeader = "X-Demo-Fail-Once";
    private const int MaximumBodyBytes = 16 * 1024;

    public static IEndpointRouteBuilder MapErpDemoOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/demo-orders", ReceiveOrder);
        return endpoints;
    }

    private static async Task<IResult> ReceiveOrder(
        HttpRequest request,
        ErpDemoReceiverDbContext dbContext,
        IOptions<ErpDemoReceiverOptions> configuredOptions,
        CancellationToken cancellationToken)
    {
        var options = configuredOptions.Value;
        if (string.IsNullOrWhiteSpace(options.SigningSecret))
        {
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Receiver signing is not configured.");
        }

        if (!TryReadTimestamp(request, options.MaximumTimestampSkewSeconds, out var timestamp) ||
            !TryReadHeader(request, SignatureHeader, 128, out var suppliedSignature) ||
            !TryReadHeader(request, IdempotencyHeader, 160, out var idempotencyKey))
        {
            return Results.Unauthorized();
        }

        if (request.ContentLength is > MaximumBodyBytes)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        string rawBody;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync(cancellationToken);
        }

        if (Encoding.UTF8.GetByteCount(rawBody) > MaximumBodyBytes)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        if (!HasValidSignature(options.SigningSecret, timestamp, rawBody, suppliedSignature))
        {
            return Results.Unauthorized();
        }

        ErpDemoOrderRequest? order;
        try
        {
            order = JsonSerializer.Deserialize<ErpDemoOrderRequest>(rawBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["body"] = ["Payload must be valid JSON."] });
        }

        var validationErrors = Validate(order);
        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var payloadHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawBody)));
        var existing = await dbContext.Receipts.SingleOrDefaultAsync(
            receipt => receipt.IdempotencyKey == idempotencyKey,
            cancellationToken);

        if (existing is not null)
        {
            return await ExistingReceiptResultAsync(existing, payloadHash, dbContext, cancellationToken);
        }

        var receivedAt = DateTime.UtcNow;
        var receipt = new ErpDemoReceipt(
            Guid.NewGuid(),
            idempotencyKey,
            payloadHash,
            order!.CustomerReference,
            order.CaseNumber,
            order.DocumentReference);
        receipt.RegisterAttempt();
        var failOnceRequested = options.EnableFailOnce &&
                                string.Equals(request.Headers[FailOnceHeader], "true", StringComparison.OrdinalIgnoreCase);
        if (failOnceRequested)
        {
            receipt.MarkFailOnceTriggered();
            dbContext.Receipts.Add(receipt);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                dbContext.ChangeTracker.Clear();
                existing = await dbContext.Receipts.SingleOrDefaultAsync(
                    stored => stored.IdempotencyKey == idempotencyKey,
                    cancellationToken);
                if (existing is not null)
                {
                    return await ExistingReceiptResultAsync(existing, payloadHash, dbContext, cancellationToken);
                }

                throw;
            }

            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Controlled ERP demo failure.");
        }

        receipt.MarkReceived(CreateExternalReceiptId(receipt.Id), receivedAt);
        dbContext.Receipts.Add(receipt);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            existing = await dbContext.Receipts.SingleOrDefaultAsync(
                stored => stored.IdempotencyKey == idempotencyKey,
                cancellationToken);
            if (existing is not null)
            {
                return await ExistingReceiptResultAsync(existing, payloadHash, dbContext, cancellationToken);
            }

            throw;
        }

        return Results.Created(
            $"/api/demo-orders/{receipt.ExternalReceiptId}",
            ToResponse(receipt, duplicate: false));
    }

    private static async Task<IResult> ExistingReceiptResultAsync(
        ErpDemoReceipt receipt,
        string payloadHash,
        ErpDemoReceiverDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(receipt.PayloadHash),
                Encoding.ASCII.GetBytes(payloadHash)))
        {
            return Results.Conflict(new { Error = "Idempotency key was already used with a different payload." });
        }

        if (receipt.ExternalReceiptId is null)
        {
            var receivedAt = DateTime.UtcNow;
            receipt.RegisterAttempt();
            receipt.MarkReceived(CreateExternalReceiptId(receipt.Id), receivedAt);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Created(
                $"/api/demo-orders/{receipt.ExternalReceiptId}",
                ToResponse(receipt, duplicate: false));
        }

        return Results.Ok(ToResponse(receipt, duplicate: true));
    }

    private static ErpDemoReceiptResponse ToResponse(ErpDemoReceipt receipt, bool duplicate) => new(
        receipt.ExternalReceiptId!,
        "Received",
        duplicate,
        receipt.ReceivedAt!.Value);

    private static bool TryReadTimestamp(HttpRequest request, int maximumSkewSeconds, out string timestamp)
    {
        timestamp = string.Empty;
        if (!TryReadHeader(request, TimestampHeader, 32, out var value) ||
            !long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            return false;
        }

        DateTimeOffset requestTime;
        try
        {
            requestTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        if ((DateTimeOffset.UtcNow - requestTime).Duration() > TimeSpan.FromSeconds(Math.Max(1, maximumSkewSeconds)))
        {
            return false;
        }

        timestamp = value;
        return true;
    }

    private static bool TryReadHeader(HttpRequest request, string name, int maximumLength, out string value)
    {
        value = request.Headers[name].ToString();
        return !string.IsNullOrWhiteSpace(value) && value.Length <= maximumLength;
    }

    private static bool HasValidSignature(string secret, string timestamp, string rawBody, string suppliedSignature)
    {
        byte[] suppliedBytes;
        try
        {
            suppliedBytes = Convert.FromHexString(suppliedSignature);
        }
        catch (FormatException)
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expectedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{rawBody}"));
        return suppliedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }

    private static Dictionary<string, string[]> Validate(ErpDemoOrderRequest? order)
    {
        var errors = new Dictionary<string, string[]>();
        if (order is null)
        {
            errors["body"] = ["Payload is required."];
            return errors;
        }

        ValidateField(errors, "customerReference", order.CustomerReference, 120, "FICTIONAL-");
        ValidateField(errors, "caseNumber", order.CaseNumber, 80, "LIVE-");
        ValidateField(errors, "documentReference", order.DocumentReference, 160);
        return errors;
    }

    private static void ValidateField(
        IDictionary<string, string[]> errors,
        string name,
        string? value,
        int maximumLength,
        string? requiredPrefix = null)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength ||
            (requiredPrefix is not null && !value.StartsWith(requiredPrefix, StringComparison.Ordinal)))
        {
            errors[name] = [$"{name} is invalid for the fictional demo receiver."];
        }
    }

    private static string CreateExternalReceiptId(Guid id) => $"ERP-DEMO-{id:N}"[..21].ToUpperInvariant();
}
