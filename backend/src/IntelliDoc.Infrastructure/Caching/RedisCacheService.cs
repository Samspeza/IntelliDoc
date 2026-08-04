using System.Text.Json;
using IntelliDoc.Application.Common.Interfaces;
using StackExchange.Redis;

namespace IntelliDoc.Infrastructure.Caching;

/// <summary>
/// Implementação de ICacheService (Application, Etapa 9.3) usando Redis
/// diretamente via IConnectionMultiplexer (em vez de
/// IDistributedCache/Microsoft.Extensions.Caching.StackExchangeRedis) para
/// ter acesso a KEYS/SCAN, necessário em RemoverPorPrefixoAsync (invalidação
/// em lote do cache do dashboard, docs/04-arquitetura.md §8).
/// </summary>
public sealed class RedisCacheService(IConnectionMultiplexer conexao) : ICacheService
{
    private IDatabase Db => conexao.GetDatabase();

    public async Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken)
    {
        var valor = await Db.StringGetAsync(chave);

        if (!valor.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(valor!);
    }

    public async Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(valor);
        await Db.StringSetAsync(chave, json, expiracao);
    }

    public async Task RemoverAsync(string chave, CancellationToken cancellationToken)
    {
        await Db.KeyDeleteAsync(chave);
    }

    public async Task RemoverPorPrefixoAsync(string prefixo, CancellationToken cancellationToken)
    {
        var endpoints = conexao.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var servidor = conexao.GetServer(endpoint);

            await foreach (var chave in servidor.KeysAsync(pattern: $"{prefixo}*"))
            {
                await Db.KeyDeleteAsync(chave);
            }
        }
    }
}