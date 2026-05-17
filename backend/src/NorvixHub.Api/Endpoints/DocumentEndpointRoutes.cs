namespace NorvixHub.Api.Endpoints;

public static partial class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents");

        group.MapGet("/", ListDocuments).WithName("ListDocuments");
        group.MapPost("/", UploadDocument).WithName("UploadDocument");
        group.MapPost("/sample", CreateSampleDocument).WithName("CreateSampleDocument");
        group.MapGet("/{id:guid}", GetDocument).WithName("GetDocument");
        group.MapGet("/{id:guid}/download", DownloadDocument).WithName("DownloadDocument");
        group.MapPost("/{id:guid}/versions", UploadVersion).WithName("UploadDocumentVersion");
        group.MapPost("/{id:guid}/link-to-case", LinkToCase).WithName("LinkDocumentToCase");
        group.MapPost("/{id:guid}/analyze", AnalyzeDocument).WithName("AnalyzeDocument");
        group.MapPost("/{id:guid}/approve-classification", ApproveClassification)
            .WithName("ApproveDocumentClassification");

        return app;
    }
}
