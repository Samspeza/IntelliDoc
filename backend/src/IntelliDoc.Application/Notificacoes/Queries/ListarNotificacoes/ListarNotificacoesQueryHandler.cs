using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Notificacoes.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Notificacoes.Queries.ListarNotificacoes;

/// <summary>
/// Executa o UC31. Uma notificação "pertence" ao usuário autenticado se:
///   (a) UsuarioDestinoId == usuário atual (notificação individual, RN30), OU
///   (b) PapelDestino é um dos papéis do usuário na empresa (agregada, RN29).
/// RN31: notificações expiram em 30 dias - aplicado aqui como filtro de
/// leitura, não como exclusão física (a limpeza física seria um job
/// periódico, fora do escopo desta v1, documentado no roadmap).
/// </summary>
public sealed class ListarNotificacoesQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<ListarNotificacoesQuery, PaginatedList<NotificacaoDto>>
{
    private const int TamanhoMaximoPagina = 100;

    public async Task<PaginatedList<NotificacaoDto>> Handle(ListarNotificacoesQuery request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UsuarioId!.Value;
        var papeisDoUsuario = currentUser.Papeis.ToList();
        var limiteExpiracao = DateTime.UtcNow.AddDays(-30); // RN31

        var query = dbContext.Notificacoes
            .Where(n => n.CriadoEm >= limiteExpiracao)
            .Where(n => n.UsuarioDestinoId == usuarioId
                || (n.PapelDestino != null && papeisDoUsuario.Contains(n.PapelDestino.Value)));

        if (request.ApenasNaoLidas)
        {
            query = query.Where(n => !n.Lida);
        }

        var tamanhoPagina = Math.Clamp(request.TamanhoPagina, 1, TamanhoMaximoPagina);
        var pagina = Math.Max(request.Pagina, 1);

        var projecao = query
            .OrderByDescending(n => n.CriadoEm)
            .Select(n => new NotificacaoDto(n.Id, n.Titulo, n.Mensagem, n.Lida, n.DocumentoRelacionadoId, n.CriadoEm));

        return await PaginatedList<NotificacaoDto>.CriarAsync(projecao, pagina, tamanhoPagina, cancellationToken);
    }
}