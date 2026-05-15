namespace NorvixHub.Contracts.Organizations;

public sealed record OrganizationSearchResultResponse(
    string OrganizationNumber,
    string Name,
    string? OrganizationForm,
    string? Municipality,
    string? PostalAddress,
    bool IsDeleted,
    DateTimeOffset SourceUpdatedAt);

