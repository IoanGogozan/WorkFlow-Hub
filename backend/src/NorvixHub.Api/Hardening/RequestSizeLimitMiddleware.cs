using Microsoft.Extensions.Options;

namespace NorvixHub.Api.Hardening;

public sealed class RequestSizeLimitMiddleware(
    RequestDelegate next,
    IOptions<RequestLimitOptions> options)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var maxRequestBodyBytes = options.Value.MaxRequestBodyBytes;
        if (maxRequestBodyBytes > 0 &&
            httpContext.Request.ContentLength is { } contentLength &&
            contentLength > maxRequestBodyBytes)
        {
            httpContext.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                error = $"Request body is too large. Maximum allowed size is {maxRequestBodyBytes} bytes."
            });
            return;
        }

        await next(httpContext);
    }
}
