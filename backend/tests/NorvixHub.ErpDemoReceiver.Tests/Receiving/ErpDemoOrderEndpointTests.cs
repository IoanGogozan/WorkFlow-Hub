using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NorvixHub.ErpDemoReceiver.Persistence;
using Xunit;

namespace NorvixHub.ErpDemoReceiver.Tests.Receiving;

public sealed class ErpDemoOrderEndpointTests
{
    private const string SigningSecret = "receiver-test-secret-with-sufficient-entropy";
    private const string ValidBody = "{\"customerReference\":\"FICTIONAL-CUSTOMER-001\",\"caseNumber\":\"LIVE-2026-TEST\",\"documentReference\":\"demo-document.pdf\"}";

    [Fact]
    public async Task First_valid_request_returns_created_receipt()
    {
        using var factory = new ReceiverFactory();
        using var client = factory.CreateClient();

        using var response = await SendSignedAsync(client, "run-created", ValidBody);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var receipt = await ReadReceiptAsync(response);
        receipt.ReceiptId.Should().StartWith("ERP-DEMO-");
        receipt.Status.Should().Be("Received");
        receipt.Duplicate.Should().BeFalse();
    }

    [Fact]
    public async Task Same_key_and_payload_returns_same_duplicate_receipt()
    {
        using var factory = new ReceiverFactory();
        using var client = factory.CreateClient();
        using var firstResponse = await SendSignedAsync(client, "run-duplicate", ValidBody);
        var first = await ReadReceiptAsync(firstResponse);

        using var duplicateResponse = await SendSignedAsync(client, "run-duplicate", ValidBody);
        var duplicate = await ReadReceiptAsync(duplicateResponse);

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        duplicate.ReceiptId.Should().Be(first.ReceiptId);
        duplicate.ReceivedAt.Should().Be(first.ReceivedAt);
        duplicate.Duplicate.Should().BeTrue();
    }

    [Fact]
    public async Task Same_key_with_different_payload_returns_conflict()
    {
        using var factory = new ReceiverFactory();
        using var client = factory.CreateClient();
        using var firstResponse = await SendSignedAsync(client, "run-conflict", ValidBody);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        const string changedBody = "{\"customerReference\":\"FICTIONAL-CUSTOMER-002\",\"caseNumber\":\"LIVE-2026-TEST\",\"documentReference\":\"demo-document.pdf\"}";

        using var response = await SendSignedAsync(client, "run-conflict", changedBody);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invalid_signature_returns_unauthorized()
    {
        using var factory = new ReceiverFactory();
        using var client = factory.CreateClient();

        using var response = await SendSignedAsync(client, "run-invalid-signature", ValidBody, signature: new string('0', 64));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Timestamp_outside_allowed_skew_returns_unauthorized()
    {
        using var factory = new ReceiverFactory();
        using var client = factory.CreateClient();
        var staleTimestamp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds().ToString();

        using var response = await SendSignedAsync(client, "run-stale", ValidBody, staleTimestamp);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Non_fictional_payload_returns_validation_error()
    {
        using var factory = new ReceiverFactory();
        using var client = factory.CreateClient();
        const string unsafeBody = "{\"customerReference\":\"CUSTOMER-001\",\"caseNumber\":\"CASE-123\",\"documentReference\":\"demo-document.pdf\"}";

        using var response = await SendSignedAsync(client, "run-validation", unsafeBody);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enabled_fail_once_returns_503_then_201_then_duplicate_200_without_extra_row()
    {
        using var factory = new ReceiverFactory(enableFailOnce: true);
        using var client = factory.CreateClient();

        using var failedResponse = await SendSignedAsync(client, "run-fail-once", ValidBody, failOnce: true);
        failedResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var failedState = await ReadStoredStateAsync(factory);
        failedState.Count.Should().Be(1);
        failedState.AttemptCount.Should().Be(1);
        failedState.FailOnceTriggered.Should().BeTrue();
        failedState.ExternalReceiptId.Should().BeNull();

        using var successfulResponse = await SendSignedAsync(client, "run-fail-once", ValidBody, failOnce: true);
        successfulResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var successfulReceipt = await ReadReceiptAsync(successfulResponse);
        successfulReceipt.Duplicate.Should().BeFalse();

        using var duplicateResponse = await SendSignedAsync(client, "run-fail-once", ValidBody, failOnce: true);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var duplicateReceipt = await ReadReceiptAsync(duplicateResponse);
        duplicateReceipt.ReceiptId.Should().Be(successfulReceipt.ReceiptId);
        duplicateReceipt.Duplicate.Should().BeTrue();

        var completedState = await ReadStoredStateAsync(factory);
        completedState.Count.Should().Be(1);
        completedState.AttemptCount.Should().Be(2);
        completedState.FailOnceTriggered.Should().BeTrue();
        completedState.ExternalReceiptId.Should().Be(successfulReceipt.ReceiptId);
    }

    [Fact]
    public async Task Disabled_fail_once_mode_ignores_demo_failure_header()
    {
        using var factory = new ReceiverFactory(enableFailOnce: false);
        using var client = factory.CreateClient();

        using var response = await SendSignedAsync(client, "run-fail-disabled", ValidBody, failOnce: true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var state = await ReadStoredStateAsync(factory);
        state.Count.Should().Be(1);
        state.AttemptCount.Should().Be(1);
        state.FailOnceTriggered.Should().BeFalse();
        state.ExternalReceiptId.Should().StartWith("ERP-DEMO-");
    }

    private static async Task<HttpResponseMessage> SendSignedAsync(
        HttpClient client,
        string idempotencyKey,
        string body,
        string? timestamp = null,
        string? signature = null,
        bool failOnce = false)
    {
        timestamp ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        signature ??= CreateSignature(timestamp, body);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/demo-orders")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Norvix-Timestamp", timestamp);
        request.Headers.Add("X-Norvix-Signature", signature);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        if (failOnce)
        {
            request.Headers.Add("X-Demo-Fail-Once", "true");
        }
        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static string CreateSignature(string timestamp, string body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SigningSecret));
        return Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{body}")));
    }

    private static async Task<ReceiptResponse> ReadReceiptAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return (await JsonSerializer.DeserializeAsync<ReceiptResponse>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            TestContext.Current.CancellationToken))!;
    }

    private sealed record ReceiptResponse(string ReceiptId, string Status, bool Duplicate, DateTime ReceivedAt);

    private static async Task<StoredState> ReadStoredStateAsync(ReceiverFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDemoReceiverDbContext>();
        var receipts = await dbContext.Receipts.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
        var receipt = receipts.Single();
        return new StoredState(
            receipts.Count,
            receipt.AttemptCount,
            receipt.FailOnceTriggered,
            receipt.ExternalReceiptId);
    }

    private sealed record StoredState(
        int Count,
        int AttemptCount,
        bool FailOnceTriggered,
        string? ExternalReceiptId);

    private sealed class ReceiverFactory : WebApplicationFactory<Program>
    {
        private readonly bool enableFailOnce;
        private readonly string databasePath = Path.Combine(
            Path.GetTempPath(),
            $"norvix-erp-endpoint-{Guid.NewGuid():N}.db");

        public ReceiverFactory(bool enableFailOnce = false)
        {
            this.enableFailOnce = enableFailOnce;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ErpDemoReceiver:SigningSecret"] = SigningSecret,
                    ["ErpDemoReceiver:MaximumTimestampSkewSeconds"] = "300",
                    ["ErpDemoReceiver:EnableFailOnce"] = enableFailOnce.ToString()
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ErpDemoReceiverDbContext>();
                services.RemoveAll<DbContextOptions<ErpDemoReceiverDbContext>>();
                services.AddDbContext<ErpDemoReceiverDbContext>(options =>
                    options.UseSqlite($"Data Source={databasePath};Pooling=False"));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                SqliteConnection.ClearAllPools();
                File.Delete(databasePath);
            }
        }
    }
}
