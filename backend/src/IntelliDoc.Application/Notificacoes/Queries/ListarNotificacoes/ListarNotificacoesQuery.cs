using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Notificacoes.Dtos;
using MediatR;

namespace IntelliDoc.Application.Notificacoes.Queries.ListarNotificacoes;

/// <summary>UC31: lista notificações do usuário autenticado (individuais e agregadas por papel, RN29/RN30).</summary>
public sealed record ListarNotificacoesQuery(
    bool ApenasNaoLidas = false,
    int Pagina = 1,
    int TamanhoPagina = 20) : IRequest<PaginatedList<NotificacaoDto>>;