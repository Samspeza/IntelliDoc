namespace IntelliDoc.Application.Documentos.Dtos;

/// <summary>
/// Projeção de HistoricoStatusDocumento (RN23) para a timeline exibida na
/// tela de detalhe do documento. Status expostos como string (nome do enum)
/// para não acoplar o frontend aos valores numéricos persistidos.
/// </summary>
public sealed record HistoricoStatusDto(
    string StatusAnterior,
    string StatusNovo,
    Guid? UsuarioId,
    string? NomeUsuario,
    string? Motivo,
    DateTime OcorridoEm);