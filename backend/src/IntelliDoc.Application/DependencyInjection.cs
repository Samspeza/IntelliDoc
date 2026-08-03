using System.Reflection;
using FluentValidation;
using IntelliDoc.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliDoc.Application;

/// <summary>
/// Ponto único de registro de serviços da camada Application no
/// contêiner de DI. Chamado por Program.cs tanto da Api quanto do Worker
/// (ambos processam Commands/Queries via MediatR - ex.: o Worker executa
/// ProcessarDocumentoCommand).
/// A ORDEM dos Behaviors importa: UnhandledException (mais externo) →
/// Logging → Validation (mais interno, mais perto do Handler real).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(assembly);

        return services;
    }
}