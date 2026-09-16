using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Notificacoes.Commands.MarcarNotificacaoComoLida;
using IntelliDoc.Application.Notificacoes.Dtos;
using IntelliDoc.Application.Notificacoes.Queries.ListarNotificacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliDoc.Api.Controllers;

[Authorize]
public sealed class NotificacoesController : ApiControllerBase
{
    /// <summary>UC31: lista as notificações do usuário autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<NotificacaoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] ListarNotificacoesQuery query, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(query, cancellationToken));

    /// <summary>UC31: marca uma notificação como lida.</summary>
    [HttpPost("{id:guid}/marcar-lida")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarComoLida(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new MarcarNotificacaoComoLidaCommand(id), cancellationToken);
        return NoContent();
    }
}