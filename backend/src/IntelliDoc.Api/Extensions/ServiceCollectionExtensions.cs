using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

namespace IntelliDoc.Api.Extensions;

/// <summary>
/// Agrupa configurações verbosas de infraestrutura web (Swagger com JWT,
/// CORS, rate limiting), mantendo Program.cs como um roteiro legível de
/// "o que a aplicação registra", não um arquivo de 200 linhas.
/// </summary>
public static class ServiceCollectionExtensions
{
    public const string PoliticaCorsFrontend = "FrontendPolicy";
    public const string PoliticaRateLimitAuth = "auth";

    /// <summary>Swagger/OpenAPI com suporte a autenticação Bearer (RNF02).</summary>
    public static IServiceCollection AddSwaggerComJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IntelliDoc API",
                Version = "v1",
                Description = "Plataforma de processamento inteligente de documentos com OCR, IA e workflow de aprovação."
            });

            var esquemaJwt = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe apenas o token JWT (sem o prefixo 'Bearer').",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", esquemaJwt);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { esquemaJwt, Array.Empty<string>() } });
        });

        return services;
    }

    /// <summary>CORS liberado apenas para a origem do frontend configurada.</summary>
    public static IServiceCollection AddCorsFrontend(this IServiceCollection services, IConfiguration configuration)
    {
        var origensPermitidas = configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>()
            ?? ["http://localhost:5173"];

        services.AddCors(options =>
        {
            options.AddPolicy(PoliticaCorsFrontend, policy => policy
                .WithOrigins(origensPermitidas)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("X-Correlation-Id"));
        });

        return services;
    }


    public static IServiceCollection AddRateLimitingAuth(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(PoliticaRateLimitAuth, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}