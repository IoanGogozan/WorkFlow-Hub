using Microsoft.EntityFrameworkCore;
using NorvixHub.Api.Auth;
using NorvixHub.Api.Endpoints;
using NorvixHub.Api.Hardening;
using NorvixHub.Api.RateLimiting;
using NorvixHub.Application.Tenancy;
using NorvixHub.Infrastructure;
using NorvixHub.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRequestHardening(builder.Configuration);
builder.Services.AddDeploymentProxyReadiness(builder.Configuration);
builder.Services.AddPublicDemoRateLimiting(builder.Configuration);
builder.Services.AddScoped<LocalDevTenantContext>();
builder.Services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<LocalDevTenantContext>());
builder.Services.AddScoped<TenantAuthorizationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NorvixHubDbContext>();
    await dbContext.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync(CancellationToken.None);
}

app.UseDeploymentProxyReadiness();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<PublicExceptionHandlingMiddleware>();
app.UseRateLimiter();
app.UseMiddleware<RequestSizeLimitMiddleware>();
app.UseMiddleware<LocalDevAuthMiddleware>();

app.MapHealthEndpoints();

if (app.Configuration.GetValue("Test:EnableExceptionProbe", false))
{
    app.MapGet("/__test/throw", () =>
    {
        throw new InvalidOperationException("Sensitive stack trace detail");
    });
    app.MapGet("/__test/request-info", (HttpContext httpContext) => new
    {
        httpContext.Request.Scheme,
        Host = httpContext.Request.Host.ToString(),
        IsHttps = httpContext.Request.IsHttps,
        RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
    });
}

app.MapDemoSessionEndpoints();
app.MapDemoStoryEndpoints();
app.MapLiveDemoRunEndpoints();
app.MapSessionEndpoints();
app.MapIntakeEndpoints();
app.MapAiReviewEndpoints();
app.MapReviewTaskEndpoints();
app.MapCaseEndpoints();
app.MapOrganizationEndpoints();
app.MapDocumentEndpoints();
app.MapIntegrationEndpoints();
app.MapDeliveryEndpoints();
app.MapAnalyticsEndpoints();

app.Run();

public partial class Program;
