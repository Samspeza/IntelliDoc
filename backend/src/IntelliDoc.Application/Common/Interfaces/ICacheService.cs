namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai o cache distribuído (Redis, conforme docs/04-arquitetura.md §8).
/// Usado para indicadores do dashboard, configurações de empresa e
/// blacklist de refresh tokens revogados.
/// </summary>
public interface ICacheService
{
    Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken);

    Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken);

    Task RemoverAsync(string chave, CancellationToken cancellationToken);

    /// <summary>Remove todas as chaves que começam com o prefixo indicado (invalidação em lote, ex.: "dashboard:{empresaId}:*").</summary>
    Task RemoverPorPrefixoAsync(string prefixo, CancellationToken cancellationToken);
}