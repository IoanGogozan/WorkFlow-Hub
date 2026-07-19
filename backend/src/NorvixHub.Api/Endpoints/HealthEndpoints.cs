using NorvixHub.Contracts.Health;

namespace NorvixHub.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => CreateResponse())
            .WithName("Health")
            .AllowAnonymous();

        app.MapGet("/health/ready", () => CreateResponse())
            .WithName("Readiness")
            .AllowAnonymous();

        app.MapGet("/health/version", (IConfiguration configuration, IHostEnvironment environment) => new
            {
                Commit = configuration["Build:GitSha"] ?? "unknown",
                BuiltAt = configuration["Build:Date"] ?? "unknown",
                Environment = environment.EnvironmentName,
                DeploymentTarget = configuration["Build:DeploymentTarget"] ?? "unknown"
            })
            .WithName("Version")
            .AllowAnonymous();

        return app;
    }

    private static HealthResponse CreateResponse() =>
        new("ok", "NorvixHub.Api", DateTimeOffset.UtcNow);
}
