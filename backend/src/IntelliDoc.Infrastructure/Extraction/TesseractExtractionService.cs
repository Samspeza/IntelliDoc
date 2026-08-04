using IntelliDoc.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tesseract;

namespace IntelliDoc.Infrastructure.Extraction;

public sealed class TesseractSettings
{
    public const string SectionName = "Tesseract";

    /// <summary>Diretório contendo os arquivos .traineddata (ex.: por.traineddata).</summary>
    public string CaminhoDadosTreinamento { get; init; } = "/data/tessdata";
    public string Idioma { get; init; } = "por";
}

/// <summary>
/// Implementação padrão de IDocumentExtractionService (Application, Etapa
/// 9.3) usando o motor Tesseract OCR para extrair texto de imagens/PDFs, e
/// AiFieldParsingService para estruturar os campos a partir do texto bruto.
/// Consumida exclusivamente pelo Worker (ProcessarDocumentoCommandHandler),
/// nunca diretamente pela Api - reforça a decisão arquitetural de manter o
/// processamento pesado fora do processo web (Etapa 4, §1).
/// </summary>
public sealed class TesseractExtractionService(
    IOptions<TesseractSettings> settings,
    AiFieldParsingService fieldParsingService,
    ILogger<TesseractExtractionService> logger) : IDocumentExtractionService
{
    private readonly TesseractSettings _settings = settings.Value;

    public Task<ResultadoExtracaoDocumento> ExtrairAsync(Stream conteudoArquivo, string tipoArquivo, CancellationToken cancellationToken)
    {
        try
        {
            using var memoria = new MemoryStream();
            conteudoArquivo.CopyTo(memoria);
            var bytes = memoria.ToArray();

            using var engine = new TesseractEngine(_settings.CaminhoDadosTreinamento, _settings.Idioma, EngineMode.Default);
            using var pix = Pix.LoadFromMemory(bytes);
            using var pagina = engine.Process(pix);

            var textoOcr = pagina.GetText();

            if (string.IsNullOrWhiteSpace(textoOcr))
            {
                throw new DocumentExtractionException("O OCR não conseguiu extrair nenhum texto do arquivo.");
            }

            var (tipoDocumento, camposBrutos) = fieldParsingService.ExtrairCampos(textoOcr);

            var resultado = new ResultadoExtracaoDocumento(textoOcr, tipoDocumento, camposBrutos);
            return Task.FromResult(resultado);
        }
        catch (Exception ex) when (ex is not DocumentExtractionException)
        {
            logger.LogError(ex, "Falha ao processar OCR para arquivo do tipo {TipoArquivo}", tipoArquivo);
            throw new DocumentExtractionException("Falha ao processar o documento via OCR.", ex);
        }
    }
}