using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Entities;
using Microsoft.Extensions.Options;

namespace IntelliDoc.Infrastructure.Identity;

/// <summary>
/// Implementação de ITokenService (Application). Delega a geração ao
/// JwtTokenService (Etapa 9.5) e monta o record TokensAutenticacao com o
/// refresh token bruto (para o cliente) e seu hash (para persistência,
/// RN10).
/// </summary>
public sealed class TokenService(JwtTokenService jwtTokenService, IOptions<JwtSettings> settings) : ITokenService
{
    private readonly JwtSettings _settings = settings.Value;

    public TokensAutenticacao EmitirTokens(Usuario usuario)
    {
        var accessToken = jwtTokenService.GerarAccessToken(usuario);
        var refreshTokenBruto = jwtTokenService.GerarRefreshTokenBruto();
        var refreshTokenHash = jwtTokenService.CalcularHash(refreshTokenBruto);

        return new TokensAutenticacao(
            AccessToken: accessToken,
            AccessTokenExpiraEm: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutos),
            RefreshTokenBruto: refreshTokenBruto,
            RefreshTokenHash: refreshTokenHash,
            RefreshTokenExpiraEm: jwtTokenService.CalcularExpiracaoRefreshToken());
    }

    public string CalcularHashRefreshToken(string refreshTokenBruto) =>
        jwtTokenService.CalcularHash(refreshTokenBruto);
}