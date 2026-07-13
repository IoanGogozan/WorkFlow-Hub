using System.Text.Json;
using FluentAssertions;
using NorvixHub.Contracts.LiveDemo;
using Xunit;

namespace NorvixHub.ContractTests.LiveDemo;

public sealed class LiveDemoContractTests
{
    [Fact]
    public void Create_request_defaults_to_a_safe_preset_scenario()
    {
        var request = new CreateLiveDemoRunRequest();

        request.SimulateErpFailureOnce.Should().BeFalse();
    }

    [Fact]
    public void Incomplete_run_keeps_result_fields_nullable_and_serializes_public_shape()
    {
        var response = new LiveDemoRunResponse(
            Guid.Parse("11111111-1111-4111-8111-111111111111"),
            "Running",
            "document-created",
            new DateTimeOffset(2026, 7, 11, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 11, 12, 0, 1, TimeSpan.Zero),
            null,
            null,
            0,
            false,
            null,
            null,
            [
                new LiveDemoRunStepResponse(
                    "brreg-checked",
                    2,
                    "Kontrollert",
                    "Brreg",
                    "Completed",
                    "live",
                    1,
                    1100,
                    "Firmadata kontrollert.",
                    "BRREG-0142",
                    null,
                    null)
            ],
            null);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Contain("\"runId\"");
        json.Should().Contain("\"evidenceMode\":\"live\"");
        json.Should().Contain("\"result\":null");
        json.Should().NotContain("clientSecret");
        json.Should().NotContain("accessToken");
        json.Should().NotContain("rawResponse");
    }

    [Fact]
    public void Result_and_capabilities_expose_only_public_safe_evidence()
    {
        var result = new LiveDemoRunResultResponse(
            "LIVE-2026-0142",
            "fallback",
            "folder-0142",
            "file-0142",
            "ERP-RECEIPT-0142",
            7,
            "/technical/live-runs/run-id",
            "/cases/case-id",
            "/documents/document-id",
            "/api/documents/document-id/download",
            "/technical/live-runs/run-id#sharepoint",
            "/technical/live-runs/run-id#audit");
        var capabilities = new LiveDemoCapabilitiesResponse(true, true, false, false, true);

        result.CaseNumber.Should().Be("LIVE-2026-0142");
        result.SharePointFolderReference.Should().Be("folder-0142");
        result.EvidenceHref.Should().Be("/technical/live-runs/run-id");
        result.DocumentDownloadHref.Should().EndWith("/download");
        capabilities.SharePointEnabled.Should().BeFalse();
        capabilities.ErpReceiverEnabled.Should().BeFalse();
    }
}
