using System.Text.Json;
using FluentAssertions;
using NorvixHub.Contracts.LiveDemoEvidence;
using Xunit;

namespace NorvixHub.ContractTests.LiveDemoEvidence;

public sealed class LiveDemoEvidenceContractTests
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    [Fact]
    public void Evidence_serializes_the_complete_public_shape()
    {
        var response = CreateResponse();

        var json = JsonSerializer.Serialize(response, JsonOptions);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.GetProperty("run").GetProperty("runId").GetString().Should().NotBeNullOrWhiteSpace();
        root.GetProperty("request").GetProperty("sourceLabel").GetString().Should().Be("Fiktiv henvendelse");
        root.GetProperty("brreg").GetProperty("mode").GetString().Should().Be("live");
        root.GetProperty("case").GetProperty("caseHref").GetString().Should().Be("/cases/case-id");
        root.GetProperty("document").GetProperty("downloadHref").GetString().Should().Contain("/download");
        root.GetProperty("sharePoint").GetProperty("operations").GetArrayLength().Should().Be(1);
        root.GetProperty("erp").ValueKind.Should().Be(JsonValueKind.Null);
        root.GetProperty("auditEvents").GetArrayLength().Should().Be(1);
        root.GetProperty("links").GetProperty("integrationDashboardHref").GetString().Should().Be("/integrations");
    }

    [Fact]
    public void Evidence_shape_does_not_define_sensitive_transport_or_storage_fields()
    {
        var json = JsonSerializer.Serialize(CreateResponse(), JsonOptions);

        json.Should().NotContain("accessToken");
        json.Should().NotContain("connectionString");
        json.Should().NotContain("storagePath");
        json.Should().NotContain("settingsJson");
        json.Should().NotContain("userAgent");
        json.Should().NotContain("ipAddress");
        json.Should().NotContain("hmacSignature");
        json.Should().NotContain("rawException");
        json.Should().NotContain("rawPayload");
    }

    private static LiveDemoEvidenceResponse CreateResponse()
    {
        var timestamp = new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero);
        var documentId = Guid.Parse("22222222-2222-4222-8222-222222222222");

        return new LiveDemoEvidenceResponse(
            new LiveDemoEvidenceRunResponse(
                Guid.Parse("11111111-1111-4111-8111-111111111111"),
                "Completed", "corr-0142", timestamp, timestamp, timestamp.AddSeconds(2),
                2000, 0, "Fiktiv servicehenvendelse"),
            new LiveDemoEvidenceRequestResponse(
                "Serviceforespørsel", "Fiktiv forespørsel.", "KUN-0142",
                "Fiktiv henvendelse", timestamp),
            new LiveDemoEvidenceBrregResponse(
                "live", "123456789", "Eksempel Drift AS", 120,
                timestamp.AddDays(-1), "Kontrollert mot Brreg."),
            new LiveDemoEvidenceCaseResponse(
                "LIVE-2026-0142", "Serviceoppdrag", "Open", "Eksempel Drift AS",
                timestamp, "/cases/case-id"),
            new LiveDemoEvidenceDocumentResponse(
                documentId, "Saksgrunnlag", "saksgrunnlag.pdf", 2048,
                "application/pdf", 1, "sha256:a1b2c3d4", timestamp,
                $"/documents/{documentId}", $"/documents/{documentId}/download"),
            new LiveDemoEvidenceSharePointResponse(
                "simulated", "Demo Site", "Dokumenter", "/Saker/LIVE-2026-0142",
                "folder-0142", "file-0142", "saksgrunnlag.pdf", 1, "etag-1",
                new Dictionary<string, string> { ["CaseNumber"] = "LIVE-2026-0142" },
                [new LiveDemoEvidenceSharePointOperationResponse(
                    timestamp, "PUT", "/content", 201, "Created", 18, 1, "created")],
                "/technical/sharepoint"),
            null,
            [new LiveDemoEvidenceAuditEventResponse(
                timestamp, "LiveDemoRunCompleted", "Kjøring fullført", "LiveDemoRun",
                "Completed", "corr-0142", "Norvix WorkFlow Hub", 20, 1)],
            new LiveDemoEvidenceLinksResponse(
                "/cases/case-id", $"/documents/{documentId}",
                $"/documents/{documentId}/download", "/delivery-packages/package-id",
                "/technical/sharepoint", "/integrations"));
    }
}
