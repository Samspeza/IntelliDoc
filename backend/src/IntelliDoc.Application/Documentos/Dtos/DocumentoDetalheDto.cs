namespace IntelliDoc.Application.Documentos.Dtos;

/// <summary>
/// Projeção completa usada no DETALHE do documento (UC15/UC20), incluindo
/// os campos extraídos e a timeline de status. TextoOcrBruto é incluído
/// para permitir que o revisor confira o texto reconhecido quando algum
/// campo estiver com baixa confiança.
/// </summary>
public sealed record DocumentoDetalheDto(
    Guid Id,
    string NomeArquivoOriginal,
    string TipoArquivo,
    long TamanhoBytes,
    string TipoDocumento,
    string Status,
    bool PrioridadeRevisao,
    decimal? ConfidenceScoreMedio,
    int TentativasProcessamento,
    string? TextoOcrBruto,
    string? MotivoRejeicao,
    bool Arquivado,
    Guid EnviadoPorUsuarioId,
    DateTime CriadoEm,
    IReadOnlyCollection<CampoExtraidoDto> Campos,
    IReadOnlyCollection<HistoricoStatusDto> Historico);