namespace NorvixHub.Application.Organizations;

public sealed record BrregOrganization(
    string OrganizationNumber,
    string Name,
    string? OrganizationForm,
    string? Municipality,
    string? PostalAddress,
    bool IsDeleted,
    DateTimeOffset SourceUpdatedAt,
    string RawJson);

