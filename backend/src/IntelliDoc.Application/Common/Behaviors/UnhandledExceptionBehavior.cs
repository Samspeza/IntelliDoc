using MediatR;
using Microsoft.Extensions.Logging;
using DomainException = IntelliDoc.Domain.Exceptions.DomainException;
using ValidationException = IntelliDoc.Application.Common.Exceptions.ValidationException;

namespace IntelliDoc.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior do MediatR: captura exceções que NÃO são erros de
/// negócio esperados (DomainException, ValidationException,
/// ForbiddenAccessException, NotFoundException já são tratadas de forma
/// específica pelo ExceptionHandlingMiddleware na Api) - ou seja, captura
/// bugs de fato (NullReferenceException, erro de conexão não tratado, etc.)
/// para logar com stack trace completo antes de deixar a exceção subir.
/// Sem este behavior, uma exceção inesperada só apareceria nos logs do
/// ASP.NET Core sem o contexto de qual Command/Query estava rodando.
/// </summary>
public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex) when (ex is not DomainException
                                     and not ValidationException
                                     and not Exceptions.ForbiddenAccessException
                                     and not Exceptions.NotFoundException)
        {
            logger.LogError(
                ex,
                "Exceção não tratada ao processar {Request}: {Message}",
                typeof(TRequest).Name, ex.Message);
            throw;
        }
    }
}