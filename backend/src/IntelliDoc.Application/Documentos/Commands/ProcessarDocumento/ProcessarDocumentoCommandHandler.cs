using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliDoc.Application.Documentos.Commands.ProcessarDocumento;

/// <summary>
/// Executa o UC14 dentro do processo Worker. Fluxo (espelha o diagrama de
/// sequência da Etapa 4, item 4):
///   1. Carrega o Documento (com a Empresa/Configuracao, para o limiar de
///      confiança - RN16) e marca IniciarProcessamento (Enviado -> Processando).
///   2. Lê o arquivo do storage.
///   3. Chama IDocumentExtractionService (OCR/IA).
///   4. Em caso de sucesso: converte o resultado bruto em CampoExtraido[] e
///      chama Documento.RegistrarResultadoExtracao (Processando -> AguardandoRevisao).
///   5. Em caso de falha: chama Documento.MarcarFalhaProcessamento (RN14) e,
///      se ainda houver tentativas disponíveis, reenfileira automaticamente.
/// Todo o método roda em uma única unidade de trabalho - um único
/// SaveChangesAsync no final - para que o SaveChangesAsync's interceptor de
/// eventos (Etapa 9.4) dispare de forma consistente.
/// </summary>
public sealed class ProcessarDocumentoCommandHandler(
    IApplicationDbContext dbContext,
    IFileStorageService fileStorage,
    IDocumentExtractionService extractionService,
    IDocumentProcessingQueue processingQueue,
    ILogger<ProcessarDocumentoCommandHandler> logger) : IRequestHandler<ProcessarDocumentoCommand>
{
    public async Task Handle(ProcessarDocumentoCommand request, CancellationToken cancellationToken)
    {
        var documento = await dbContext.Documentos
            .FirstOrDefaultAsync(d => d.Id == request.DocumentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Documento), request.DocumentoId);

        var empresa = await dbContext.Empresas
            .Include(e => e.Configuracao)
            .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Empresa), documento.EmpresaId);

        documento.IniciarProcessamento();
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await using var arquivo = await fileStorage.ObterAsync(documento.CaminhoArmazenamento, cancellationToken);
            var resultado = await extractionService.ExtrairAsync(arquivo, documento.TipoArquivo, cancellationToken);

            var camposExtraidos = resultado.Campos
                .Select(c => CampoExtraido.Criar(documento.Id, c.NomeCampo, c.Valor, c.ConfidenceScore))
                .ToList();

            var tipoClassificado = Enum.TryParse<TipoDocumento>(resultado.TipoDocumentoClassificado, out var tipo)
                ? tipo
                : TipoDocumento.NaoClassificado;

            documento.RegistrarResultadoExtracao(camposExtraidos, tipoClassificado, empresa.Configuracao.LimiarConfiancaIa);

            logger.LogInformation(
                "Documento {DocumentoId} processado com sucesso. Score médio: {Score}, Prioridade: {Prioridade}",
                documento.Id, documento.ScoreMedio?.Valor, documento.PrioridadeRevisao);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao processar documento {DocumentoId} (tentativa {Tentativa})",
                documento.Id, documento.TentativasProcessamento + 1);

            documento.MarcarFalhaProcessamento();

            if (documento.PodeReprocessar())
            {
                // RN14: retry automático até o limite de 3 tentativas.
                // Reenfileira ANTES do SaveChangesAsync final não é seguro
                // (o job poderia rodar antes do commit); por isso o
                // reenfileiramento acontece depois, já fora do try/catch.
                await dbContext.SaveChangesAsync(cancellationToken);
                await processingQueue.EnfileirarProcessamentoAsync(documento.Id, cancellationToken);
                return;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}