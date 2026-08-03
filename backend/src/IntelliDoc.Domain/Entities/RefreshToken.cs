using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Exceptions;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Entidade filha do agregado Usuario. Representa uma sessão de refresh
/// token (RN10, docs/02-regras-de-negocio.md). O token em si nunca é
/// armazenado em texto plano - apenas seu hash (TokenHash), gerado pela
/// Infrastructure (JwtTokenService).
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public DateTime? RevogadoEm { get; private set; }

    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    private RefreshToken(Guid usuarioId, string tokenHash, DateTime expiraEm)
    {
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        ExpiraEm = expiraEm;
    }

    internal static RefreshToken Criar(Guid usuarioId, string tokenHash, DateTime expiraEm)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new RegraDeNegocioException("REFRESH_TOKEN_HASH_OBRIGATORIO", "O hash do refresh token é obrigatório.");
        }

        if (expiraEm <= DateTime.UtcNow)
        {
            throw new RegraDeNegocioException("REFRESH_TOKEN_EXPIRACAO_INVALIDA", "A data de expiração deve ser futura.");
        }

        return new RefreshToken(usuarioId, tokenHash, expiraEm);
    }

    public bool EstaValido() => RevogadoEm is null && ExpiraEm > DateTime.UtcNow;

    public void Revogar() => RevogadoEm ??= DateTime.UtcNow;
}