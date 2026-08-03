namespace IntelliDoc.Application.Common.Exceptions;

/// <summary>
/// Lançada quando um Command/Query Handler não encontra a entidade
/// solicitada (já filtrada pelo Global Query Filter de tenant, RN02 - ou
/// seja, também é lançada quando a entidade existe mas pertence a outra
/// empresa, o que é o comportamento de segurança desejado: não revelar que
/// o recurso existe em outro tenant). Traduzida para HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string nomeEntidade, object chave)
        : base($"Entidade '{nomeEntidade}' ({chave}) não foi encontrada.")
    {
    }
}