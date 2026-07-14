using System.Net;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Infrastructure.LiveDemo;
using Xunit;

namespace NorvixHub.UnitTests.LiveDemo;

public sealed class ErpDemoClientTests
{
    private const string SigningSecret = "main-app-test-signing-secret";
    private static readonly Guid RunId = Guid.Parse("11111111-2222-4333-8444-555555555555");

    [Fact]
    public async Task Created_response_is_mapped_and_request_is_canonical_and_signed()
    {
        string? capturedBody = null;
        using var httpClient = new HttpClient(new StubHandler(async (request, cancellationToken) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            request.Headers.GetValues("Idempotency-Key").Single()
                .Should().Be("live-demo-11111111222243338444555555555555");
            request.Headers.GetValues("X-Demo-Fail-Once").Single().Should().Be("true");
            var timestamp = request.Headers.GetValues("X-Norvix-Timestamp").Single();
            var suppliedSignature = request.Headers.GetValues("X-Norvix-Signature").Single();
            suppliedSignature.Should().Be(CreateSignature(timestamp, capturedBody));
            return ReceiptResponse(HttpStatusCode.Created, duplicate: false);
        }));
        var client = CreateClient(httpClient);

        var result = await client.SendAsync(CreateRequest(failOnce: true), TestContext.Current.CancellationToken);

        result.Status.Should().Be(ErpDemoResultStatus.Received);
        result.ReceiptId.Should().Be("ERP-DEMO-TEST1234");
        result.Duplicate.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        capturedBody.Should().Be(
            "{\"customerReference\":\"FICTIONAL-CUSTOMER-001\",\"caseNumber\":\"LIVE-2026-TEST\",\"documentReference\":\"demo-document.pdf\",\"runId\":\"11111111-2222-4333-8444-555555555555\"}");
    }

    [Fact]
    public async Task Duplicate_ok_response_is_mapped()
    {
        using var httpClient = RespondWith(ReceiptResponse(HttpStatusCode.OK, duplicate: true));
        var client = CreateClient(httpClient);

        var result = await client.SendAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(ErpDemoResultStatus.Received);
        result.Duplicate.Should().BeTrue();
        result.ReceiptId.Should().Be("ERP-DEMO-TEST1234");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, ErpDemoResultStatus.Unauthorized, false)]
    [InlineData(HttpStatusCode.Conflict, ErpDemoResultStatus.Conflict, false)]
    [InlineData(HttpStatusCode.ServiceUnavailable, ErpDemoResultStatus.Unavailable, true)]
    public async Task Error_status_is_mapped_without_reading_sensitive_response(
        HttpStatusCode statusCode,
        ErpDemoResultStatus expectedStatus,
        bool retryable)
    {
        using var httpClient = RespondWith(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("sensitive upstream detail must not be mapped")
        });
        var client = CreateClient(httpClient);

        var result = await client.SendAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(expectedStatus);
        result.IsRetryable.Should().Be(retryable);
        result.ReceiptId.Should().BeNull();
    }

    [Fact]
    public async Task Timeout_is_mapped_as_retryable()
    {
        using var httpClient = new HttpClient(new StubHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));
        var client = CreateClient(httpClient, timeoutSeconds: 1);

        var result = await client.SendAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(ErpDemoResultStatus.Timeout);
        result.IsRetryable.Should().BeTrue();
    }

    private static ErpDemoClient CreateClient(HttpClient httpClient, int timeoutSeconds = 10) => new(
        httpClient,
        Options.Create(new ErpDemoOptions
        {
            BaseUrl = "https://erp-receiver.test/",
            SigningSecret = SigningSecret,
            TimeoutSeconds = timeoutSeconds
        }));

    private static ErpDemoRequest CreateRequest(bool failOnce = false) => new(
        RunId,
        "FICTIONAL-CUSTOMER-001",
        "LIVE-2026-TEST",
        "demo-document.pdf",
        failOnce);

    private static HttpClient RespondWith(HttpResponseMessage response) => new(
        new StubHandler((_, _) => Task.FromResult(response)));

    private static HttpResponseMessage ReceiptResponse(HttpStatusCode statusCode, bool duplicate) => new(statusCode)
    {
        Content = new StringContent(
            $$"""
            {
              "receiptId": "ERP-DEMO-TEST1234",
              "status": "Received",
              "duplicate": {{duplicate.ToString().ToLowerInvariant()}},
              "receivedAt": "2026-07-13T10:00:00Z"
            }
            """,
            Encoding.UTF8,
            "application/json")
    };

    private static string CreateSignature(string timestamp, string body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SigningSecret));
        return Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{body}")));
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => respond(request, cancellationToken);
    }
}
