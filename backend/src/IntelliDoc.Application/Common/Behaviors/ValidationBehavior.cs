using FluentValidation;
using MediatR;
using ValidationException = IntelliDoc.Application.Common.Exceptions.ValidationException;

namespace IntelliDoc.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior do MediatR: roda antes de QUALQUER Handler. Coleta
/// todos os IValidator&lt;TRequest&gt; registrados via
/// FluentValidation.DependencyInjectionExtensions (um por Command/Query,
/// ex.: UploadDocumentoCommandValidator) e, se houver falhas, interrompe o
/// pipeline lançando ValidationException - o Handler nunca chega a executar
/// com dados inválidos.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var resultados = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var falhas = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (falhas.Count != 0)
        {
            throw new ValidationException(falhas);
        }

        return await next();
    }
}