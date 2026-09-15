using ApiServiceCollectionExtensions = IntelliDoc.Api.Extensions.ServiceCollectionExtensions;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Identidade.Commands.Login;
using IntelliDoc.Application.Identidade.Commands.Logout;
using IntelliDoc.Application.Identidade.Commands.RefreshToken;
using IntelliDoc.Application.Identidade.Commands.RegistrarEmpresa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IntelliDoc.Api.Controllers;

[AllowAnonymous]
[EnableRateLimiting(ApiServiceCollectionExtensions.PoliticaRateLimitAuth)]
public sealed class AuthController : ApiControllerBase
{
    /// <summary>UC01: cadastra uma nova empresa e seu administrador inicial.</summary>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(RespostaAutenticacao), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(RegistrarEmpresaCommand command, CancellationToken cancellationToken)
    {
        var resposta = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, resposta);
    }

    /// <summary>UC02: autentica e retorna access token + refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespostaAutenticacao), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(command, cancellationToken));

    /// <summary>UC03: renova a sessão (com rotação do refresh token).</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RespostaAutenticacao), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(command, cancellationToken));

    /// <summary>UC06: encerra a sessão. Exige usuário autenticado.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
    {
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}