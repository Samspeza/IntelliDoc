using AutoMapper;
using AutoMapper.QueryableExtensions;
using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Documentos.Dtos;
using IntelliDoc.Domain.Enums;
using MediatR;

namespace IntelliDoc.Application.Documentos.Queries.ListarDocumentosPendentesRevisao;

/// <summary>
/// Executa o UC19. Ordenação em duas chaves: primeiro PrioridadeRevisao
/// (descendente - prioritários no topo, RN16), depois CriadoEm (ascendente
/// - mais antigos primeiro, evitando que documentos "envelheçam" no fim da
/// fila). Essa ordenação é exatamente o que o índice composto
/// IX_Documentos_Empresa_Prioridade_Status (Etapa 8) suporta.
///
/// Só Revisor e Gestor acessam a fila (RN18) - validado aqui, na
/// Application, pois depende dos papéis do usuário autenticado.
/// </summary>
public sealed class ListarDocumentosPendentesRevisaoQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<ListarDocumentosPendentesRevisaoQuery, PaginatedList<DocumentoResumoDto>>
{
    private const int TamanhoMaximoPagina = 100;

    public async Task<PaginatedList<DocumentoResumoDto>> Handle(
        ListarDocumentosPendentesRevisaoQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.TemPapel(PapelUsuario.Revisor) && !currentUser.TemPapel(PapelUsuario.Gestor))
        {
            throw new ForbiddenAccessException("Apenas revisores e gestores podem acessar a fila de revisão.");
        }

        var tamanhoPagina = Math.Clamp(request.TamanhoPagina, 1, TamanhoMaximoPagina);
        var pagina = Math.Max(request.Pagina, 1);

        var projecao = dbContext.Documentos
            .Where(d => d.Status == StatusDocumento.AguardandoRevisao && !d.Arquivado)
            .OrderByDescending(d => d.PrioridadeRevisao)
            .ThenBy(d => d.CriadoEm)
            .ProjectTo<DocumentoResumoDto>(mapper.ConfigurationProvider);

        return await PaginatedList<DocumentoResumoDto>.CriarAsync(projecao, pagina, tamanhoPagina, cancellationToken);
    }
}