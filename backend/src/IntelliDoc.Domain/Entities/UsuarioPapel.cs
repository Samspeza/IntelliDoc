using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Enums;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Entidade filha do agregado Usuario. Representa a associação de um papel
/// (RBAC) a um usuário - RN05/RN06. Não possui repositório próprio.
/// </summary>
public sealed class UsuarioPapel : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public PapelUsuario Papel { get; private set; }

    private UsuarioPapel()
    {
    }

    private UsuarioPapel(Guid usuarioId, PapelUsuario papel)
    {
        UsuarioId = usuarioId;
        Papel = papel;
    }

    internal static UsuarioPapel Criar(Guid usuarioId, PapelUsuario papel) => new(usuarioId, papel);
}