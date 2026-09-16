using IntelliDoc.Application.Dashboard.Dtos;
using IntelliDoc.Application.Dashboard.Queries.ObterIndicadores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliDoc.Api.Controllers;

[Authorize]
public sealed class DashboardController : ApiControllerBase
{
    /// <summary>UC25/UC26: indicadores agregados, com filtro de período opcional.</summary>
    [HttpGet("indicadores")]
    [ProducesResponseType(typeof(IndicadoresDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterIndicadores(
        [FromQuery] ObterIndicadoresQuery query, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(query, cancellationToken));
}