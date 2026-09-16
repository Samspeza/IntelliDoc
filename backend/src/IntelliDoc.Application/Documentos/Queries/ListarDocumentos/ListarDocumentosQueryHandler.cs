using AutoMapper;
using AutoMapper.QueryableExtensions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Documentos.Dtos;
using MediatR;

namespace IntelliDoc.Application.Documentos.Queries.ListarDocumentos;

/// <summary>
/// Executa o UC16. Usa ProjectTo (AutoMapper.QueryableExtensions) em vez de
/// carregar entidades e mapear em memória: o SQL gerado seleciona APENAS as
/// colunas presentes em DocumentoResumoDto, sem trazer TextoOcrBruto (um
/// campo text potencialmente grande) nem executar JOINs para Campos e
/// Historico. Em uma listagem paginada, essa diferença é expressiva.
/// O isolamento por empresa é automático (Global Query Filter, RN02).
/// </summary>
public sealed class ListarDocumentosQueryHandler(
    IApplicationDbContext dbContext,
    IMapper mapper) : IRequestHandler<ListarDocumentosQuery, PaginatedList<DocumentoResumoDto>>
{
    private const int TamanhoMaximoPagina = 100;

    public async Task<PaginatedList<DocumentoResumoDto>> Handle(ListarDocumentosQuery request, CancellationToken cancellationToken)
    {
        var tamanhoPagina = Math.Clamp(request.TamanhoPagina, 1, TamanhoMaximoPagina);
        var pagina = Math.Max(request.Pagina, 1);

        var query = dbContext.Documentos.AsQueryable();

        if (!request.IncluirArquivados)
        {
            query = query.Where(d => !d.Arquivado);
        }

        if (request.Status is not null)
        {
            query = query.Where(d => d.Status == request.Status);
        }

        if (request.TipoDocumento is not null)
        {
            query = query.Where(d => d.TipoDocumento == request.TipoDocumento);
        }

        if (request.PeriodoInicio is not null)
        {
            query = query.Where(d => d.CriadoEm >= request.PeriodoInicio);
        }

        if (request.PeriodoFim is not null)
        {
            query = query.Where(d => d.CriadoEm <= request.PeriodoFim);
        }

        var projecao = query
            .OrderByDescending(d => d.CriadoEm)
            .ProjectTo<DocumentoResumoDto>(mapper.ConfigurationProvider);

        return await PaginatedList<DocumentoResumoDto>.CriarAsync(projecao, pagina, tamanhoPagina, cancellationToken);
    }
}