namespace NorvixHub.Application.LiveDemo;

public interface ILiveDemoOrganizationResolver
{
    Task<LiveDemoOrganizationResolution> ResolveAsync(
        string organizationNumber,
        CancellationToken cancellationToken);
}

public sealed record LiveDemoOrganizationResolution(
    string Mode,
    LiveDemoOrganizationData Organization,
    long DurationMs,
    string? SafeReason);

public sealed record LiveDemoOrganizationData(
    string OrganizationNumber,
    string Name,
    string? OrganizationForm,
    string? Municipality,
    DateTimeOffset SourceUpdatedAt);

public sealed class LiveDemoOrganizationResolutionException : Exception
{
    public LiveDemoOrganizationResolutionException(string code, string publicMessage)
        : base(publicMessage)
    {
        Code = code;
        PublicMessage = publicMessage;
    }

    public string Code { get; }
    public string PublicMessage { get; }
}
