namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai a fila de processamento assíncrono de documentos (RF07/RNF03).
/// Implementada por Infrastructure.Queue.HangfireDocumentProcessingQueue.
/// Isola a decisão "Hangfire em vez de RabbitMQ/Kafka" (docs/04-arquitetura.md
/// §3) do restante do sistema - trocar de provedor de fila no futuro não
/// exige alterar nenhum Command Handler.
/// </summary>
public interface IDocumentProcessingQueue
{
    /// <summary>Enfileira o processamento (OCR/IA) do documento indicado.</summary>
    Task EnfileirarProcessamentoAsync(Guid documentoId, CancellationToken cancellationToken);
}