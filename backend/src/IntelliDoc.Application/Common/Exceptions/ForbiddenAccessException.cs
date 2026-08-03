namespace IntelliDoc.Application.Common.Exceptions;

/// <summary>
/// Lançada por um Command/Query Handler quando ICurrentUserService indica
/// que o usuário autenticado não possui o papel necessário para a ação
/// (ex.: RN18 - só Revisor/Gestor podem aprovar documentos). Traduzida pelo
/// ExceptionHandlingMiddleware para HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("Você não tem permissão para executar esta ação.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}