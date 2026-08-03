namespace IntelliDoc.Domain.Exceptions;

/// <summary>
/// Exception base para violações de regras de domínio. É capturada
/// especificamente pelo ExceptionHandlingMiddleware (Api) e traduzida para
/// HTTP 400 Bad Request, diferenciando-se de erros técnicos inesperados
/// (500 Internal Server Error).
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}