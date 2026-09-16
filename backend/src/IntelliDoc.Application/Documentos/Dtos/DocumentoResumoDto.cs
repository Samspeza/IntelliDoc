namespace IntelliDoc.Application.Documentos.Dtos;

/// <summary>
/// Projeção enxuta usada nas LISTAGENS (UC16, UC19). Propositalmente não
/// inclui campos extraídos, histórico nem o texto OCR - carregar isso em
/// uma lista de 50 itens multiplicaria o tráfego sem que a tela use os
/// dados. O detalhe completo vem de DocumentoDetalheDto (UC15).
/// </summary>
public sealed record DocumentoResumoDto(
    Guid Id,
    string NomeArquivoOriginal,
    string TipoArquivo,
    string TipoDocumento,
    string Status,
    bool PrioridadeRevisao,
    decimal? ConfidenceScoreMedio,
    Guid EnviadoPorUsuarioId,
    DateTime CriadoEm);