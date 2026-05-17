namespace NorvixHub.Api.Hardening;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        httpContext.Response.OnStarting(() =>
        {
            var headers = httpContext.Response.Headers;
            headers.TryAdd("X-Content-Type-Options", "nosniff");
            headers.TryAdd("X-Frame-Options", "DENY");
            headers.TryAdd("Referrer-Policy", "no-referrer");
            headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");
            headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
            headers.TryAdd("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'");
            return Task.CompletedTask;
        });

        await next(httpContext);
    }
}
