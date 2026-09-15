using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Entities;

namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai a emissão de tokens de autenticação (UC02/UC03, RN10).
/// Implementada por Infrastructure.Identity.TokenService, que delega ao
/// JwtTokenService (Etapa 9.5). Esta porta existe para que os Command
/// Handlers do módulo de Identidade não dependam diretamente de uma classe
/// concreta da Infrastructure - mantendo-os testáveis com um fake simples.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Emite o par access token + refresh token. O refresh token BRUTO é
    /// retornado ao cliente, mas apenas seu hash é persistido (RN10) - o
    /// Handler é responsável por chamar Usuario.EmitirRefreshToken com o
    /// hash retornado em TokensAutenticacao.RefreshTokenHash.
    /// </summary>
    TokensAutenticacao EmitirTokens(Usuario usuario);

    string CalcularHashRefreshToken(string refreshTokenBruto);
}