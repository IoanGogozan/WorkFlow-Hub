namespace NorvixHub.Api.Endpoints;

public static partial class IntegrationEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/integrations");

        group.MapGet("/", ListIntegrations).WithName("ListIntegrations");
        group.MapGet("/{provider}", GetIntegration).WithName("GetIntegration");
        group.MapPost("/{provider}/connect", ConnectIntegration).WithName("ConnectIntegration");
        group.MapPost("/{provider}/disconnect", DisconnectIntegration).WithName("DisconnectIntegration");
        group.MapPost("/{provider}/sync", SyncIntegration).WithName("SyncIntegration");
        group.MapGet("/{provider}/sync-runs", ListSyncRuns).WithName("ListIntegrationSyncRuns");
        group.MapPost("/{provider}/sync-runs/{syncRunId:guid}/retry", RetrySyncRun)
            .WithName("RetryIntegrationSyncRun");

        return app;
    }
}
