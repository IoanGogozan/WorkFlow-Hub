namespace NorvixHub.Api.Endpoints;

public static partial class DeliveryEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cases/{caseId:guid}/delivery-packages", CreatePackage)
            .WithName("CreateDeliveryPackage");
        app.MapGet("/api/delivery-packages/{id:guid}", GetPackage).WithName("GetDeliveryPackage");
        app.MapPost("/api/delivery-packages/{id:guid}/generate-pdf", GeneratePdf)
            .WithName("GenerateDeliveryPdf");
        app.MapPost("/api/delivery-packages/{id:guid}/create-link", CreateLink)
            .WithName("CreateDeliveryLink");
        app.MapPost("/api/delivery-links/{id:guid}/revoke", RevokeLink).WithName("RevokeDeliveryLink");

        app.MapGet("/delivery/{token}", OpenDelivery).WithName("OpenPublicDelivery");
        app.MapGet("/delivery/{token}/documents/{documentId:guid}", OpenDeliveryDocument)
            .WithName("OpenPublicDeliveryDocument");

        return app;
    }
}
