namespace IntelliDoc.Domain.Events;

/// <summary>
/// Disparado por Documento.Aprovar(). Um handler na camada Application
/// (Notificacoes) escuta este evento para criar a Notificacao ao operador
/// que enviou o documento (UC30), mantendo o agregado Documento desacoplado
/// da lógica de notificação - Documento não sabe que Notificacao existe.
/// </summary>
public sealed class DocumentoAprovadoEvent : DomainEvent
{
    public Guid DocumentoId { get; }
    public Guid EmpresaId { get; }
    public Guid EnviadoPorUsuarioId { get; }
    public Guid AprovadoPorUsuarioId { get; }
    public string NomeArquivoOriginal { get; }

    public DocumentoAprovadoEvent(
        Guid documentoId,
        Guid empresaId,
        Guid enviadoPorUsuarioId,
        Guid aprovadoPorUsuarioId,
        string nomeArquivoOriginal)
    {
        DocumentoId = documentoId;
        EmpresaId = empresaId;
        EnviadoPorUsuarioId = enviadoPorUsuarioId;
        AprovadoPorUsuarioId = aprovadoPorUsuarioId;
        NomeArquivoOriginal = nomeArquivoOriginal;
    }
}