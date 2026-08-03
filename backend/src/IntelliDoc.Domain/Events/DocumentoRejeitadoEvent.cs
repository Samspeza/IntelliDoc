namespace IntelliDoc.Domain.Events;

/// <summary>
/// Disparado por Documento.Rejeitar(). Análogo a DocumentoAprovadoEvent,
/// mas carrega também o motivo da rejeição (RN21), que é exibido na
/// notificação enviada ao operador (UC30).
/// </summary>
public sealed class DocumentoRejeitadoEvent : DomainEvent
{
    public Guid DocumentoId { get; }
    public Guid EmpresaId { get; }
    public Guid EnviadoPorUsuarioId { get; }
    public Guid RejeitadoPorUsuarioId { get; }
    public string NomeArquivoOriginal { get; }
    public string Motivo { get; }

    public DocumentoRejeitadoEvent(
        Guid documentoId,
        Guid empresaId,
        Guid enviadoPorUsuarioId,
        Guid rejeitadoPorUsuarioId,
        string nomeArquivoOriginal,
        string motivo)
    {
        DocumentoId = documentoId;
        EmpresaId = empresaId;
        EnviadoPorUsuarioId = enviadoPorUsuarioId;
        RejeitadoPorUsuarioId = rejeitadoPorUsuarioId;
        NomeArquivoOriginal = nomeArquivoOriginal;
        Motivo = motivo;
    }
}