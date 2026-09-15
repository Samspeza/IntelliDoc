using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IntelliDoc.Api.Controllers;

/// <summary>
/// Base de todos os Controllers. Expõe o IMediator resolvido sob demanda
/// (evita repetir a injeção em cada Controller) e centraliza as convenções
/// de rota e content-type.
///
/// Os Controllers desta API são propositalmente FINOS: recebem o HTTP,
/// montam o Command/Query e devolvem o resultado. Nenhuma regra de negócio,
/// nenhuma consulta a banco, nenhum try/catch (tratado pelo
/// ExceptionHandlingMiddleware).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}