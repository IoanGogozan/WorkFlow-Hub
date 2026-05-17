using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace NorvixHub.Api.Hardening;

public sealed class PublicExceptionHandlingMiddleware(
    RequestDelegate next,
    IWebHostEnvironment environment,
    ILogger<PublicExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception exception) when (!environment.IsDevelopment())
        {
            logger.LogError(
                exception,
                "Unhandled request exception. TraceIdentifier: {TraceIdentifier}",
                httpContext.TraceIdentifier);

            if (httpContext.Response.HasStarted)
            {
                throw;
            }

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "The request could not be completed.",
                Instance = httpContext.TraceIdentifier
            };

            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                problem,
                cancellationToken: httpContext.RequestAborted);
        }
    }
}
