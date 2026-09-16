using IntelliDoc.Application.Auditoria.Dtos;
using IntelliDoc.Application.Auditoria.Queries.ListarAuditoria;
using IntelliDoc.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliDoc.Api.Controllers;

[Authorize]
public sealed class AuditoriaController : ApiControllerBase
{
    /// <summary>UC33: consulta a trilha de auditoria (restrito a AdminEmpresa/SuperAdmin).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<RegistroAuditoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Listar([FromQuery] ListarAuditoriaQuery query, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(query, cancellationToken));
}