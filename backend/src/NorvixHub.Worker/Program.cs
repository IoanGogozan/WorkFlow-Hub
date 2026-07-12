using NorvixHub.Worker;
using NorvixHub.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<LiveDemoRunWorker>();

var host = builder.Build();
host.Run();
