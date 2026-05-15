namespace NorvixHub.Application.Organizations;

public interface IBrregClient
{
    Task<IReadOnlyList<BrregOrganization>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<BrregOrganization?> GetByOrganizationNumberAsync(string organizationNumber, CancellationToken cancellationToken);
}

