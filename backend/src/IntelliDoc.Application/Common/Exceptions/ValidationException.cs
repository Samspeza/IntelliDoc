using FluentValidation.Results;

namespace IntelliDoc.Application.Common.Exceptions;

/// <summary>
/// Lançada pelo ValidationBehavior quando um ou mais Validators (FluentValidation)
/// falham para um Command/Query. Agrega os erros por propriedade, no formato
/// que o ExceptionHandlingMiddleware (Api) espera para montar a resposta
/// HTTP 400 (ProblemDetails).
/// </summary>
public sealed class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("Um ou mais erros de validação ocorreram.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}