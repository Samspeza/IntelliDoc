using IntelliDoc.Application;
using IntelliDoc.Infrastructure;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// --- Serilog (RNF05, docs/04-arquitetura.md §7) ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Processo", "Worker")
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// --- Application + Infrastructure (mesmo registro usado pela Api, exceto
//     autenticação JWT - o Worker não expõe endpoints HTTP) ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Ativa o processamento efetivo dos jobs Hangfire (Etapa 9.5) ---
// É esta linha que diferencia o Worker da Api: só aqui o Hangfire Server
// roda, consumindo ProcessarDocumentoCommand da fila via IMediator.
builder.Services.AddWorkerHangfireServer();

var host = builder.Build();

Log.Information("IntelliDoc.Worker iniciado - aguardando jobs de processamento de documentos.");

host.Run();