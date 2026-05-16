using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NorvixHub.Infrastructure.Organizations;
using Xunit;

namespace NorvixHub.UnitTests.Organizations;

public sealed class BrregClientTests
{
    [Fact]
    public async Task Get_by_organization_number_sets_base_address_before_first_request()
    {
        using var httpClient = new HttpClient(new StubHandler(request =>
        {
            request.RequestUri!.IsAbsoluteUri.Should().BeTrue();
            request.RequestUri.ToString().Should().Be("https://example.test/enheter/999888777");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "organisasjonsnummer": "999888777",
                      "navn": "Agder Drift & Service AS",
                      "organisasjonsform": { "kode": "AS" },
                      "erSlettet": false
                    }
                    """)
            };
        }));
        var client = new BrregClient(
            httpClient,
            Options.Create(new BrregOptions { BaseUrl = "https://example.test/" }));

        var organization = await client.GetByOrganizationNumberAsync(
            "999888777",
            TestContext.Current.CancellationToken);

        organization!.Name.Should().Be("Agder Drift & Service AS");
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(respond(request));
        }
    }
}
