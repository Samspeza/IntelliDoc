namespace IntelliDoc.Application.Common.Models;

/// <summary>
/// Par de tokens emitido no login (UC02) e na renovação (UC03).
/// RefreshTokenBruto vai para o cliente; RefreshTokenHash é o que o Handler
/// persiste via Usuario.EmitirRefreshToken (RN10).
/// </summary>
public sealed record TokensAutenticacao(
    string AccessToken,
    DateTime AccessTokenExpiraEm,
    string RefreshTokenBruto,
    string RefreshTokenHash,
    DateTime RefreshTokenExpiraEm);

/// <summary>Resposta devolvida ao cliente nos endpoints de login/refresh.</summary>
public sealed record RespostaAutenticacao(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEm,
    Guid UsuarioId,
    string Nome,
    string Email,
    Guid? EmpresaId,
    IReadOnlyCollection<string> Papeis);