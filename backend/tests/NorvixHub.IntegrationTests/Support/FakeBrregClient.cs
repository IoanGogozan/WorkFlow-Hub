using NorvixHub.Application.Organizations;

namespace NorvixHub.IntegrationTests.Support;

public sealed class FakeBrregClient : IBrregClient
{
    private static readonly BrregOrganization Organization = new(
        "999888777",
        "Sordal Eiendom AS",
        "AS",
        "Kristiansand",
        "Markens gate 1, 4610 Kristiansand",
        false,
        DateTimeOffset.Parse("2026-05-15T00:00:00Z"),
        """{"organisasjonsnummer":"999888777","navn":"Sordal Eiendom AS"}""");

    public Task<IReadOnlyList<BrregOrganization>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BrregOrganization> result =
            query.Contains("sordal", StringComparison.OrdinalIgnoreCase) || query == Organization.OrganizationNumber
                ? new[] { Organization }
                : Array.Empty<BrregOrganization>();
        return Task.FromResult(result);
    }

    public Task<BrregOrganization?> GetByOrganizationNumberAsync(
        string organizationNumber,
        CancellationToken cancellationToken)
    {
        var result = organizationNumber == Organization.OrganizationNumber ? Organization : null;
        return Task.FromResult(result);
    }
}

