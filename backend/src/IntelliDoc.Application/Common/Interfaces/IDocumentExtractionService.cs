namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai o provedor de OCR + extração estruturada de dados (RF08/RF09).
/// Implementada por padrão por Infrastructure.Extraction.TesseractExtractionService
/// + AiFieldParsingService, mas pode ser trocada (Azure Document Intelligence,
/// API multimodal, etc.) sem alterar Application/Domain - decisão registrada
/// em docs/04-arquitetura.md (§3, nota "Escopo da IA/OCR").
/// Consumida pelo Worker (ProcessarDocumentoCommandHandler), nunca pela Api.
/// </summary>
public interface IDocumentExtractionService
{
    /// <summary>
    /// Executa OCR + extração de campos estruturados sobre o arquivo
    /// indicado. Lança DocumentExtractionException (definida junto à
    /// implementação) em caso de falha - capturada pelo Handler para
    /// acionar Documento.MarcarFalhaProcessamento (RN14).
    /// </summary>
    Task<ResultadoExtracaoDocumento> ExtrairAsync(Stream conteudoArquivo, string tipoArquivo, CancellationToken cancellationToken);
}

/// <summary>Resultado bruto retornado pelo provedor de OCR/IA, antes de virar entidades CampoExtraido.</summary>
public sealed record ResultadoExtracaoDocumento(
    string TextoOcrBruto,
    string TipoDocumentoClassificado, // nome do enum TipoDocumento como string
    IReadOnlyList<CampoExtraidoBruto> Campos);

public sealed record CampoExtraidoBruto(string NomeCampo, string? Valor, decimal ConfidenceScore);