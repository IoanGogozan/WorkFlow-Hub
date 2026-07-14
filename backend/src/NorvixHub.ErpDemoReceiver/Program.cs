using Microsoft.EntityFrameworkCore;
using NorvixHub.ErpDemoReceiver.Persistence;
using NorvixHub.ErpDemoReceiver.Receiving;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddErpDemoPersistence(builder.Configuration);
builder.Services.Configure<ErpDemoReceiverOptions>(
    builder.Configuration.GetSection(ErpDemoReceiverOptions.SectionName));

var app = builder.Build();

const string serviceDescription = "Norvix ERP demo receiver — fictional integration target";

await app.InitializeErpDemoPersistenceAsync();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = serviceDescription
}));

app.MapGet("/health/ready", async (ErpDemoReceiverDbContext dbContext) =>
    await dbContext.Database.CanConnectAsync()
        ? Results.Ok(new { Status = "Ready", Service = serviceDescription })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapErpDemoOrderEndpoints();

app.Run();

public partial class Program;
