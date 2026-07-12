using System.Diagnostics;
using Microsoft.Extensions.Options;
using NorvixHub.Application.LiveDemo;
using NorvixHub.Application.Organizations;

namespace NorvixHub.Infrastructure.LiveDemo;

public sealed class LiveDemoOrganizationResolver(
    IBrregClient brregClient,
    IOptions<LiveDemoOptions> options) : ILiveDemoOrganizationResolver
{
    private const string UnavailableCode = "BRREG_UNAVAILABLE";
    private const string UnavailableMessage = "Organisasjonsinformasjon er ikke tilgjengelig akkurat nå.";

    public async Task<LiveDemoOrganizationResolution> ResolveAsync(
        string organizationNumber,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                var organization = await brregClient.GetByOrganizationNumberAsync(
                    organizationNumber,
                    cancellationToken);
                if (organization is null)
                {
                    throw new LiveDemoOrganizationResolutionException(UnavailableCode, UnavailableMessage);
                }

                return new LiveDemoOrganizationResolution(
                    "live",
                    new LiveDemoOrganizationData(
                        organization.OrganizationNumber,
                        organization.Name,
                        organization.OrganizationForm,
                        organization.Municipality,
                        organization.SourceUpdatedAt),
                    stopwatch.ElapsedMilliseconds,
                    null);
            }
            catch (LiveDemoOrganizationResolutionException)
            {
                throw;
            }
            catch (Exception exception) when (IsTransient(exception, cancellationToken) && attempt == 0)
            {
                continue;
            }
            catch (Exception exception) when (IsTransient(exception, cancellationToken) && options.Value.BrregFallbackEnabled)
            {
                return CreateFallback(organizationNumber, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                throw new LiveDemoOrganizationResolutionException(UnavailableCode, UnavailableMessage);
            }
        }

        throw new LiveDemoOrganizationResolutionException(UnavailableCode, UnavailableMessage);
    }

    private LiveDemoOrganizationResolution CreateFallback(string organizationNumber, long durationMs)
    {
        var configuration = options.Value;
        return new LiveDemoOrganizationResolution(
            "fallback",
            new LiveDemoOrganizationData(
                organizationNumber,
                configuration.BrregFallbackOrganizationName,
                configuration.BrregFallbackOrganizationForm,
                configuration.BrregFallbackMunicipality,
                configuration.BrregFallbackSourceUpdatedAt),
            durationMs,
            "Brreg svarte ikke innenfor den avgrensede demoforespørselen.");
    }

    private static bool IsTransient(Exception exception, CancellationToken cancellationToken) =>
        exception is HttpRequestException or TimeoutException ||
        exception is OperationCanceledException && !cancellationToken.IsCancellationRequested;
}
