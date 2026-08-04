using MediatR;

namespace IntelliDoc.Application.Documentos.Commands.ProcessarDocumento;

/// <summary>
/// UC14 (docs/03-casos-de-uso.md): Sistema (Worker) processa o documento da
/// fila. Enfileirado por IDocumentProcessingQueue.EnfileirarProcessamentoAsync
/// (chamado por UploadDocumentoCommandHandler e por ReenviarDocumentoCommandHandler)
/// e consumido pelo Hangfire Server rodando no processo Worker.
/// IRequest&lt;Unit&gt; (sem retorno) - o resultado do processamento é
/// consultado depois via ObterDocumentoPorIdQuery, não retornado aqui.
/// </summary>
public sealed record ProcessarDocumentoCommand(Guid DocumentoId) : IRequest;