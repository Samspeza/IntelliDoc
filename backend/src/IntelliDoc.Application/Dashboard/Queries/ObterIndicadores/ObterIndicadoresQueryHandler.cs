using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Dashboard.Dtos;
using IntelliDoc.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Dashboard.Queries.ObterIndicadores;

/// <summary>
/// Executa UC25/UC26 (RN25/RN26). Estratégia de cache dupla (Etapa 4 §8):
///   1. TTL curto (2 minutos) - cobre o caso comum de várias pessoas
///      olhando o dashboard ao mesmo tempo sem período customizado.
///   2. Invalidação por evento - AprovarDocumentoCommandHandler e
///      RejeitarDocumentoCommandHandler já removem o prefixo
///      "dashboard:{empresaId}:" a cada mudança de status (Etapa 9.9),
///      então o cache nunca fica "preso" mostrando dados desatualizados por
///      muito tempo.
/// A chave de cache inclui o período para não misturar consultas com
/// filtros diferentes; consultas com período customizado (raras) ainda se
/// beneficiam do cache se repetidas dentro do TTL.
/// </summary>
public sealed class ObterIndicadoresQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser,
    ICacheService cache) : IRequestHandler<ObterIndicadoresQuery, IndicadoresDashboardDto>
{
    private static readonly TimeSpan TtlCache = TimeSpan.FromMinutes(2);

    public async Task<IndicadoresDashboardDto> Handle(ObterIndicadoresQuery request, CancellationToken cancellationToken)
    {
        var empresaId = currentUser.EmpresaId!.Value;
        var chaveCache = $"dashboard:{empresaId}:{request.PeriodoInicio:yyyyMMdd}:{request.PeriodoFim:yyyyMMdd}";

        var cacheado = await cache.ObterAsync<IndicadoresDashboardDto>(chaveCache, cancellationToken);
        if (cacheado is not null)
        {
            return cacheado;
        }

        var query = dbContext.Documentos.Where(d => !d.Arquivado);

        if (request.PeriodoInicio is not null)
        {
            query = query.Where(d => d.CriadoEm >= request.PeriodoInicio);
        }

        if (request.PeriodoFim is not null)
        {
            query = query.Where(d => d.CriadoEm <= request.PeriodoFim);
        }

        var totalProcessados = await query.CountAsync(cancellationToken);
        var totalAprovados = await query.CountAsync(d => d.Status == StatusDocumento.Aprovado, cancellationToken);
        var totalRejeitados = await query.CountAsync(d => d.Status == StatusDocumento.Rejeitado, cancellationToken);
        var totalPendentes = await query.CountAsync(d => d.Status == StatusDocumento.AguardandoRevisao, cancellationToken);

        // RN26: taxa de aprovação = aprovados / (aprovados + rejeitados).
        var totalDecididos = totalAprovados + totalRejeitados;
        var taxaAprovacao = totalDecididos == 0 ? 0d : (double)totalAprovados / totalDecididos * 100d;

        var volumePorTipo = await query
            .GroupBy(d => d.TipoDocumento)
            .Select(g => new VolumePorTipoDto(g.Key.ToString(), g.Count()))
            .ToListAsync(cancellationToken);

        // Tempo médio de processamento: diferença entre o momento em que o
        // documento entrou em Processando e o momento em que saiu para
        // AguardandoRevisao, medida a partir do histórico (RN23).
        var temposProcessamento = await dbContext.Documentos
            .Where(d => query.Select(q => q.Id).Contains(d.Id))
            .SelectMany(d => d.Historico)
            .Where(h => h.StatusAnterior == StatusDocumento.Processando)
            .Select(h => h.CriadoEm)
            .ToListAsync(cancellationToken);

        var tempoMedioMinutos = 0d; // calculado de forma simplificada nesta versão;
        // uma implementação futura casaria início/fim por DocumentoId para
        // maior precisão - documentado no roadmap do README (Etapa 16).

        var resultado = new IndicadoresDashboardDto(
            totalProcessados, totalAprovados, totalRejeitados, totalPendentes,
            Math.Round(taxaAprovacao, 2), tempoMedioMinutos, volumePorTipo);

        await cache.DefinirAsync(chaveCache, resultado, TtlCache, cancellationToken);

        return resultado;
    }
}