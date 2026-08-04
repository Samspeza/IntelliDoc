using Hangfire;
using Hangfire.Dashboard;
using IntelliDoc.Api.Middlewares;
using IntelliDoc.Application;
using IntelliDoc.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog (RNF05, docs/04-arquitetura.md 7)
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Processo", "Api")
    .WriteTo.Console());


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInfrastructureAuth(builder.Configuration);
// Nota: AddWorkerHangfireServer() NÃO é chamado aqui - a Api enfileira e
// visualiza jobs, mas nunca os processa (Etapa 9.6).

// Controllers + Swagger/OpenAPI (RNF02)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IntelliDoc API",
        Version = "v1",
        Description = "Plataforma de processamento inteligente de documentos com OCR/IA e fluxo de aprovação."
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Informe: Bearer {seu token}"
    };
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme } }, [] }
    });
});


// CORS 
const string CorsPolicyFrontend = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyFrontend, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ---------------------------------------------------------------------
// Health Checks (RNF05) - banco, Redis e a própria Api
// ---------------------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

var app = builder.Build();

// ---------------------------------------------------------------------
// Pipeline
// ---------------------------------------------------------------------
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IntelliDoc API v1"));
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicyFrontend);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Dashboard do Hangfire (RNF05) - protegido: só usuários autenticados com
// papel AdminEmpresa ou SuperAdmin podem visualizar (filtro customizado
// seria adicionado em Extensions/; simplificado aqui para .RequireAuthorization()).
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilterPlaceholder()]
}).RequireAuthorization();

app.Run();

// Placeholder mínimo - a Etapa 9.8+ pode substituir por um filtro real que
// valida o papel do usuário (AdminEmpresa/SuperAdmin) antes de autorizar o
// dashboard; por ora, delega inteiramente para [Authorize] via
// RequireAuthorization() acima, que já exige um JWT válido.
sealed class HangfireAuthorizationFilterPlaceholder : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}