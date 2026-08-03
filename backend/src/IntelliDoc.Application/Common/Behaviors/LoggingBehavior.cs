using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntelliDoc.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior do MediatR: loga início/fim de todo Command/Query,
/// com duração em milissegundos, contribuindo para a observabilidade
/// (RNF05, docs/04-arquitetura.md §7). Requisições acima de 500ms são
/// logadas como Warning, para facilitar identificar gargalos em produção
/// sem precisar de uma ferramenta de APM completa.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int LimiteMilissegundosAlerta = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var nomeRequest = typeof(TRequest).Name;
        var cronometro = Stopwatch.StartNew();

        logger.LogInformation("Iniciando {Request}", nomeRequest);

        try
        {
            var resposta = await next();
            cronometro.Stop();

            if (cronometro.ElapsedMilliseconds > LimiteMilissegundosAlerta)
            {
                logger.LogWarning(
                    "{Request} concluído em {ElapsedMs}ms (acima do limite de {Limite}ms)",
                    nomeRequest, cronometro.ElapsedMilliseconds, LimiteMilissegundosAlerta);
            }
            else
            {
                logger.LogInformation("{Request} concluído em {ElapsedMs}ms", nomeRequest, cronometro.ElapsedMilliseconds);
            }

            return resposta;
        }
        catch
        {
            cronometro.Stop();
            logger.LogWarning("{Request} falhou após {ElapsedMs}ms", nomeRequest, cronometro.ElapsedMilliseconds);
            throw;
        }
    }
}