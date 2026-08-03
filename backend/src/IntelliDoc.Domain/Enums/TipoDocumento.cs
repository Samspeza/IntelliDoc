namespace IntelliDoc.Domain.Enums;

/// <summary>
/// Tipos de documento reconhecidos pela classificação automática (RF10).
/// Valores alinhados com o CHECK constraint em 001_create_schema.sql.
/// </summary>
public enum TipoDocumento
{
    NotaFiscal = 0,
    Recibo = 1,
    Contrato = 2,
    Outro = 3,
    NaoClassificado = 4
}