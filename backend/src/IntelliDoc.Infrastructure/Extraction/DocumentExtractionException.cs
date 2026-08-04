namespace IntelliDoc.Infrastructure.Extraction;

/// <summary>
/// Lançada por TesseractExtractionService quando o OCR falha (arquivo
/// corrompido, formato inesperado, engine indisponível). Capturada pelo
/// ProcessarDocumentoCommandHandler (Application, Etapa 9.6) para acionar
/// Documento.MarcarFalhaProcessamento (RN14) em vez de deixar a exceção
/// derrubar o job do Worker sem contexto de negócio.
/// </summary>
public sealed class DocumentExtractionException : Exception
{
    public DocumentExtractionException(string message) : base(message)
    {
    }

    public DocumentExtractionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}