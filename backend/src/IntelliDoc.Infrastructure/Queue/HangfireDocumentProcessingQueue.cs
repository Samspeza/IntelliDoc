using Hangfire;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Documentos.Commands.ProcessarDocumento;
using MediatR;

namespace IntelliDoc.Infrastructure.Queue;

/// <summary>
/// Implementação de IDocumentProcessingQueue (Application, Etapa 9.3) usando
/// Hangfire (docs/04-arquitetura.md §3). BackgroundJob.Enqueue serializa a
/// chamada e a persiste na tabela de jobs do Hangfire (armazenamento
/// PostgreSQL, RNF03) - mesmo que a Api ou o Worker reiniciem, o job não se
/// perde. O Worker (Etapa 9.6) é quem efetivamente processa o job, chamando
/// ProcessarDocumentoCommand via MediatR.
/// Retries automáticos do Hangfire ficam desabilitados aqui (Attempts = 0)
/// porque o retry de negócio (RN14, máx. 3 tentativas) é controlado
/// explicitamente por Documento.PodeReprocessar - não queremos dois
/// mecanismos de retry sobrepostos.
/// </summary>
public sealed class HangfireDocumentProcessingQueue : IDocumentProcessingQueue
{
    public Task EnfileirarProcessamentoAsync(Guid documentoId, CancellationToken cancellationToken)
    {
        BackgroundJob.Enqueue<IMediator>(mediator =>
            mediator.Send(new ProcessarDocumentoCommand(documentoId), CancellationToken.None));

        return Task.CompletedTask;
    }
}