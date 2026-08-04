using IntelliDoc.Application.Documentos.Commands.UploadDocumento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliDoc.Api.Controllers;

/// <summary>
/// Endpoints do módulo Documentos. Cada action mapeia diretamente a um
/// Command/Query do IntelliDoc.Application (Etapa 3, tabela "Mapeamento
/// Caso de Uso -> Camada de Aplicação"). O Controller não contém NENHUMA
/// lógica de negócio - apenas traduz HTTP <-> MediatR.
/// </summary>
[ApiController]
[Route("api/documentos")]
[Authorize] // exige JWT válido; checagem de PAPEL específico fica em cada Handler
public sealed class DocumentosController(IMediator mediator) : ControllerBase
{
    /// <summary>UC13: upload de um documento (multipart/form-data).</summary>
    /// <response code="201">Documento recebido e enfileirado para processamento.</response>
    /// <response code="400">Arquivo inválido (tipo não aceito ou acima de 10MB - RN11).</response>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // RN11 - reforça em nível de infraestrutura HTTP o limite já validado no Command
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile arquivo, CancellationToken cancellationToken)
    {
        var extensao = Path.GetExtension(arquivo.FileName).TrimStart('.').ToLowerInvariant();

        await using var stream = arquivo.OpenReadStream();

        var command = new UploadDocumentoCommand(
            stream,
            arquivo.FileName,
            extensao,
            arquivo.Length);

        var documentoId = await mediator.Send(command, cancellationToken);

        // TODO(Etapa 9.8): trocar por CreatedAtAction apontando para
        // ObterDocumentoPorId (UC15) assim que esse endpoint existir.
        return Created($"/api/documentos/{documentoId}", new { id = documentoId });
    }
}