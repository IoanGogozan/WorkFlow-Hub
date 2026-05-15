using FluentAssertions;
using NorvixHub.Contracts.Health;
using Xunit;

namespace NorvixHub.ContractTests.Health;

public sealed class HealthResponseTests
{
    [Fact]
    public void Health_response_keeps_service_status_and_timestamp()
    {
        var checkedAt = DateTimeOffset.UtcNow;
        var response = new HealthResponse("ok", "NorvixHub.Api", checkedAt);

        response.Status.Should().Be("ok");
        response.Service.Should().Be("NorvixHub.Api");
        response.CheckedAt.Should().Be(checkedAt);
    }
}
