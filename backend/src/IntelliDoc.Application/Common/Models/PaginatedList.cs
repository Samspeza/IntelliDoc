using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Common.Models;

/// <summary>
/// Envelope de paginação usado por todas as Queries de listagem (UC16 -
/// ListarDocumentos, UC19 - ListarDocumentosPendentesRevisao, UC33 -
/// ListarAuditoria, etc.), evitando reimplementar a lógica de Skip/Take +
/// contagem total em cada Query Handler.
/// </summary>
public sealed class PaginatedList<T>
{
    public IReadOnlyCollection<T> Itens { get; }
    public int PaginaAtual { get; }
    public int TotalPaginas { get; }
    public int TotalItens { get; }

    public PaginatedList(IReadOnlyCollection<T> itens, int totalItens, int paginaAtual, int tamanhoPagina)
    {
        Itens = itens;
        TotalItens = totalItens;
        PaginaAtual = paginaAtual;
        TotalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);
    }

    public bool TemPaginaAnterior => PaginaAtual > 1;
    public bool TemProximaPagina => PaginaAtual < TotalPaginas;

    public static async Task<PaginatedList<T>> CriarAsync(IQueryable<T> query, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var totalItens = await query.CountAsync(cancellationToken);

        var itens = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(itens, totalItens, pagina, tamanhoPagina);
    }
}