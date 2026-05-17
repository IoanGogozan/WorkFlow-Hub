using Microsoft.AspNetCore.Http.Features;

namespace NorvixHub.Api.Hardening;

public static class RequestHardeningExtensions
{
    public static IServiceCollection AddRequestHardening(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RequestLimitOptions>(configuration.GetSection("RequestLimits"));
        var options = configuration
            .GetSection("RequestLimits")
            .Get<RequestLimitOptions>() ?? new RequestLimitOptions();

        services.Configure<FormOptions>(formOptions =>
        {
            formOptions.MultipartBodyLengthLimit = options.MaxRequestBodyBytes;
            formOptions.ValueLengthLimit = 16 * 1024;
            formOptions.ValueCountLimit = 32;
            formOptions.MultipartHeadersLengthLimit = 16 * 1024;
        });

        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(
            kestrelOptions =>
            {
                kestrelOptions.Limits.MaxRequestBodySize = options.MaxRequestBodyBytes;
            });

        return services;
    }
}
