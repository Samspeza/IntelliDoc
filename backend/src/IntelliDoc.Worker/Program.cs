using IntelliDoc.Application;
using IntelliDoc.Infrastructure;
using Serilog;
using Hangfire;

var builder = Host.CreateApplicationBuilder(args);

// --- Serilog (RNF05, docs/04-arquitetura.md §7) ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Processo", "Worker")
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHangfireServer();

var host = builder.Build();

Log.Information("IntelliDoc.Worker iniciado - aguardando jobs de processamento de documentos.");

host.Run();