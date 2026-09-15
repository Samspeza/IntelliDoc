using IntelliDoc.Api.Authorization;
using IntelliDoc.Api.Extensions;
using IntelliDoc.Api.Middlewares;
using IntelliDoc.Application;
using IntelliDoc.Infrastructure;
using IntelliDoc.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Logging estruturado (RNF05, docs/04-arquitetura.md §7)
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Processo", "Api")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// ---------------------------------------------------------------------------
// Camadas da aplicação (Clean Architecture, Etapa 4)
// ---------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInfrastructureAuth(builder.Configuration);
// NOTA: AddWorkerHangfireServer() NÃO é chamado aqui de propósito - a Api
// apenas ENFILEIRA jobs; quem os processa é o Worker (Etapa 9.6).

// ---------------------------------------------------------------------------
// Infraestrutura web
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddSwaggerComJwt();
builder.Services.AddCorsFrontend(builder.Configuration);
builder.Services.AddRateLimitingAuth();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgres")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

var app = builder.Build();

// ---------------------------------------------------------------------------
// Migrations automáticas apenas em Development (em produção, migrations são
// aplicadas explicitamente no pipeline de CI/CD - Etapa 15 - para evitar que
// duas réplicas da Api tentem migrar o banco simultaneamente no startup).
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    using var escopo = app.Services.CreateScope();
    var dbContext = escopo.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ---------------------------------------------------------------------------
// Pipeline HTTP - a ORDEM importa
// ---------------------------------------------------------------------------

// 1. Exceções primeiro: precisa envolver todo o resto do pipeline para
//    capturar qualquer falha, inclusive de middlewares posteriores.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Correlação: logo em seguida, para que todo log subsequente já carregue
//    o CorrelationId.
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IntelliDoc API v1");
        options.DocumentTitle = "IntelliDoc API";
    });
}

app.UseHttpsRedirection();
app.UseCors(IntelliDoc.Api.Extensions.ServiceCollectionExtensions.PoliticaCorsFrontend);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

// Dashboard de jobs, protegido (Etapa 4 §7) - somente SuperAdmin/AdminEmpresa.
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});

try
{
    Log.Information("IntelliDoc.Api iniciando...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A API encerrou de forma inesperada.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// Exposto como public partial para permitir que os testes de integração
/// (IntelliDoc.IntegrationTests, Etapa 13) instanciem a aplicação via
/// WebApplicationFactory&lt;Program&gt;.
/// </summary>
public partial class Program;