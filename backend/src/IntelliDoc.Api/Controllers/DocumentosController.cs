using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Documentos.Commands.AprovarDocumento;
using IntelliDoc.Application.Documentos.Commands.CorrigirCamposExtraidos;
using IntelliDoc.Application.Documentos.Commands.RejeitarDocumento;
using IntelliDoc.Application.Documentos.Commands.UploadDocumento;
using IntelliDoc.Application.Documentos.Dtos;
using IntelliDoc.Application.Documentos.Queries.ListarDocumentos;
using IntelliDoc.Application.Documentos.Queries.ListarDocumentosPendentesRevisao;
using IntelliDoc.Application.Documentos.Queries.ObterDocumentoPorId;
using IntelliDoc.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliDoc.Api.Controllers;

/// <summary>
/// Endpoints do módulo de Documentos: upload (UC13), consulta (UC15/UC16),
/// fila de revisão (UC19) e fluxo de aprovação (UC21-UC23).
/// </summary>
[Authorize]
public sealed class DocumentosController : ApiControllerBase
{
    /// <summary>
    /// UC13: faz upload de um documento para processamento assíncrono.
    /// Retorna 202 Accepted (não 201 Created) porque o recurso ainda NÃO
    /// está em seu estado final - o OCR/IA roda de forma assíncrona no
    /// Worker, e o cliente deve consultar o status depois (RF11).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{nameof(PapelUsuario.Operador)},{nameof(PapelUsuario.AdminEmpresa)},{nameof(PapelUsuario.Gestor)}")]
    [RequestSizeLimit(10 * 1024 * 1024)] // RN11
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

    /// <summary>UC16: lista documentos da empresa com filtros opcionais.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<DocumentoResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] ListarDocumentosQuery query, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(query, cancellationToken));

    /// <summary>UC19: fila de documentos aguardando revisão (prioritários primeiro).</summary>
    [HttpGet("pendentes-revisao")]
    [ProducesResponseType(typeof(PaginatedList<DocumentoResumoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarPendentesRevisao(
        [FromQuery] ListarDocumentosPendentesRevisaoQuery query, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(query, cancellationToken));

    /// <summary>UC15/UC20: detalhe do documento, com campos extraídos e timeline.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new ObterDocumentoPorIdQuery(id), cancellationToken));

    /// <summary>UC21: corrige um ou mais campos extraídos pela IA.</summary>
    [HttpPatch("{id:guid}/campos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CorrigirCampos(
        Guid id, [FromBody] IReadOnlyCollection<CorrecaoCampo> correcoes, CancellationToken cancellationToken)
    {
        await Mediator.Send(new CorrigirCamposExtraidosCommand(id, correcoes), cancellationToken);
        return NoContent();
    }

    /// <summary>UC22: aprova o documento.</summary>
    [HttpPost("{id:guid}/aprovar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Aprovar(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new AprovarDocumentoCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>UC23: rejeita o documento, com motivo obrigatório (RN21).</summary>
    [HttpPost("{id:guid}/rejeitar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rejeitar(
        Guid id, [FromBody] RejeitarDocumentoRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new RejeitarDocumentoCommand(id, request.Motivo), cancellationToken);
        return NoContent();
    }
}

/// <summary>Resposta do upload (UC13) - o status é consultado separadamente via UC15.</summary>
public sealed record UploadDocumentoResponse(Guid DocumentoId, string Mensagem);

/// <summary>Corpo da requisição de rejeição (o Id vem pela rota).</summary>
public sealed record RejeitarDocumentoRequest(string Motivo);