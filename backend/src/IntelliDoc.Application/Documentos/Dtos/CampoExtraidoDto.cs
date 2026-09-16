namespace IntelliDoc.Application.Documentos.Dtos;

/// <summary>
/// Projeção de CampoExtraido para a tela de revisão lado a lado (UC20).
/// Expõe tanto o valor original da IA quanto o valor final (RN20), para que
/// o revisor veja o que foi corrigido e o frontend possa destacar
/// divergências.
/// </summary>
public sealed record CampoExtraidoDto(
    Guid Id,
    string NomeCampo,
    string? ValorExtraidoIa,
    string? ValorFinal,
    decimal ConfidenceScore,
    bool CorrigidoManualmente);