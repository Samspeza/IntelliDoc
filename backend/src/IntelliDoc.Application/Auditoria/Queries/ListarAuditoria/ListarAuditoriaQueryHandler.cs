using IntelliDoc.Application.Auditoria.Dtos;
using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Enums;
using MediatR;

namespace IntelliDoc.Application.Auditoria.Queries.ListarAuditoria;

/// <summary>
/// Executa o UC33. Restrito a AdminEmpresa/SuperAdmin - a trilha de
/// auditoria pode conter informações sensíveis (Ips, ações de outros
/// usuários) que não devem ser visíveis a Operadores/Revisores comuns.
/// Ordenado por CriadoEm decrescente, aproveitando o índice
/// IX_Auditoria_Empresa_CriadoEm (Etapa 8).
/// </summary>
public sealed class ListarAuditoriaQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<ListarAuditoriaQuery, PaginatedList<RegistroAuditoriaDto>>
{
    private const int TamanhoMaximoPagina = 100;

    public async Task<PaginatedList<RegistroAuditoriaDto>> Handle(ListarAuditoriaQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.TemPapel(PapelUsuario.AdminEmpresa) && !currentUser.TemPapel(PapelUsuario.SuperAdmin))
        {
            throw new ForbiddenAccessException("Apenas administradores podem consultar a trilha de auditoria.");
        }

        var query = dbContext.RegistrosAuditoria.AsQueryable();

        if (request.Acao is not null)
        {
            query = query.Where(a => a.Acao == request.Acao);
        }

        if (request.PeriodoInicio is not null)
        {
            query = query.Where(a => a.CriadoEm >= request.PeriodoInicio);
        }

        if (request.PeriodoFim is not null)
        {
            query = query.Where(a => a.CriadoEm <= request.PeriodoFim);
        }

        var tamanhoPagina = Math.Clamp(request.TamanhoPagina, 1, TamanhoMaximoPagina);
        var pagina = Math.Max(request.Pagina, 1);

        var projecao = query
            .OrderByDescending(a => a.CriadoEm)
            .Select(a => new RegistroAuditoriaDto(
                a.Id, a.UsuarioId, a.Acao.ToString(), a.EntidadeAfetada, a.EntidadeAfetadaId,
                a.DadosAntesJson, a.DadosDepoisJson, a.EnderecoIp, a.CriadoEm));

        return await PaginatedList<RegistroAuditoriaDto>.CriarAsync(projecao, pagina, tamanhoPagina, cancellationToken);
    }
}