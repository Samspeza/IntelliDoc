using System.Text;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Infrastructure.Caching;
using IntelliDoc.Infrastructure.Email;
using IntelliDoc.Infrastructure.Extraction;
using IntelliDoc.Infrastructure.Identity;
using IntelliDoc.Infrastructure.Persistence;
using IntelliDoc.Infrastructure.Persistence.Interceptors;
using IntelliDoc.Infrastructure.Queue;
using IntelliDoc.Infrastructure.Storage;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace IntelliDoc.Infrastructure;

/// <summary>
/// Ponto único de registro de serviços da camada Infrastructure. Chamado
/// por Program.cs tanto da Api quanto do Worker - ambos precisam do
/// ApplicationDbContext, mas só a Api precisa da autenticação JWT
/// (AddAuthentication), por isso esta é separada em dois métodos.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Registrado por Api e Worker: persistência, portas de infraestrutura, Hangfire, Redis.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDocumentProcessingQueue, HangfireDocumentProcessingQueue>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IDocumentExtractionService, TesseractExtractionService>();
        services.AddSingleton<AiFieldParsingService>();
        services.AddSingleton<JwtTokenService>();

        services.Configure<Identity.JwtSettings>(configuration.GetSection(Identity.JwtSettings.SectionName));
        services.Configure<LocalStorageSettings>(configuration.GetSection(LocalStorageSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<TesseractSettings>(configuration.GetSection(TesseractSettings.SectionName));

        // --- Redis (Etapa 4 §8) ---
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' não configurada.");

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ICacheService, RedisCacheService>();

        // --- Hangfire (fila de processamento, Etapa 4 §3) ---
        services.AddHangfire(config => config
            .UsePostgreSqlStorage(hf => hf.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));
        services.AddHangfireServer();

        return services;
    }

    /// <summary>Registrado apenas pela Api: autenticação/autorização via JWT Bearer.</summary>
    public static IServiceCollection AddInfrastructureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(Identity.JwtSettings.SectionName);
        var jwtSettings = jwtSection.Get<Identity.JwtSettings>() ?? new Identity.JwtSettings();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Emissor,
                    ValidAudience = jwtSettings.Audiencia,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.ChaveSecreta)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }
}