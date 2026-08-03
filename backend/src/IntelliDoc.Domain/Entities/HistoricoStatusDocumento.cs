using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Enums;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Entidade filha do agregado Documento. Registro imutável de cada
/// transição de status (RN23). Não possui repositório próprio, não é criada
/// diretamente - apenas por Documento.TransicionarStatus. Não expõe nenhum
/// método de alteração após a criação, reforçando a imutabilidade exigida
/// por RN23/RN33.
/// </summary>
public sealed class HistoricoStatusDocumento : BaseEntity
{
    public Guid DocumentoId { get; private set; }
    public StatusDocumento StatusAnterior { get; private set; }
    public StatusDocumento StatusNovo { get; private set; }
    public Guid? UsuarioId { get; private set; } // nulo quando a transição é automática (Worker)
    public string? Motivo { get; private set; }

    private HistoricoStatusDocumento()
    {
    }

    private HistoricoStatusDocumento(
        Guid documentoId,
        StatusDocumento statusAnterior,
        StatusDocumento statusNovo,
        Guid? usuarioId,
        string? motivo)
    {
        DocumentoId = documentoId;
        StatusAnterior = statusAnterior;
        StatusNovo = statusNovo;
        UsuarioId = usuarioId;
        Motivo = motivo;
    }

    internal static HistoricoStatusDocumento Criar(
        Guid documentoId,
        StatusDocumento statusAnterior,
        StatusDocumento statusNovo,
        Guid? usuarioId,
        string? motivo) => new(documentoId, statusAnterior, statusNovo, usuarioId, motivo);
}