using IntelliDoc.Application.Documentos.Commands.UploadDocumento;
using IntelliDoc.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliDoc.Api.Controllers;

/// <summary>
/// Endpoints do módulo de Documentos. Nesta sub-etapa expõe apenas o UC13
/// (upload); os demais casos de uso do módulo (UC15-UC18, UC19-UC23) serão
/// adicionados junto com seus respectivos Commands/Queries nas próximas
/// sub-etapas.
/// </summary>
[Authorize]
public sealed class DocumentosController : ApiControllerBase
{
    /// <summary>
    /// UC13: faz upload de um documento para processamento assíncrono.
    /// Retorna 202 Accepted (não 201 Created) porque o recurso ainda NÃO
    /// está em seu estado final - o OCR/IA roda de forma assíncrona no
    /// Worker, e o cliente deve consultar o status depois (RF11). Este é o
    /// código semanticamente correto para processamento assíncrono.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{nameof(PapelUsuario.Operador)},{nameof(PapelUsuario.AdminEmpresa)},{nameof(PapelUsuario.Gestor)}")]
    [RequestSizeLimit(10 * 1024 * 1024)] // RN11 - 10MB, barrado antes de carregar o corpo inteiro
    [ProducesResponseType(typeof(UploadDocumentoResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upload(IFormFile arquivo, CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Arquivo obrigatório",
                Detail = "Nenhum arquivo foi enviado na requisição.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await using var stream = arquivo.OpenReadStream();

        var tipoArquivo = Path.GetExtension(arquivo.FileName).TrimStart('.').ToLowerInvariant();

        var documentoId = await Mediator.Send(
            new UploadDocumentoCommand(stream, arquivo.FileName, tipoArquivo, arquivo.Length),
            cancellationToken);

        return Accepted(new UploadDocumentoResponse(documentoId, "Documento recebido e enfileirado para processamento."));
    }
}

/// <summary>Resposta do upload (UC13) - o status é consultado separadamente via UC15.</summary>
public sealed record UploadDocumentoResponse(Guid DocumentoId, string Mensagem);