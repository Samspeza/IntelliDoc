using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IntelliDoc.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IntelliDoc.Infrastructure.Identity;

/// <summary>
/// Gera o access token (JWT) e o refresh token usados pelo fluxo de
/// autenticação (UC02/UC03, RN09/RN10). Não implementa uma interface da
/// Application porque é consumido apenas por Command Handlers do módulo
/// Identidade, que residem junto a este projeto de infraestrutura via
/// injeção direta - diferente das outras portas (IEmailService,
/// IFileStorageService), a Application não precisa "trocar" o provedor de
/// token sem reescrever o próprio fluxo de login, então não há ganho em
/// abstrair além do necessário (evitando abstração especulativa).
/// </summary>
public sealed class JwtTokenService(IOptions<JwtSettings> settings)
{
    private readonly JwtSettings _settings = settings.Value;

    /// <summary>
    /// Gera o access token contendo: sub (UsuarioId), empresa_id (se houver),
    /// e uma claim "role" por papel do usuário (RN06 - múltiplos papéis).
    /// </summary>
    public string GerarAccessToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email.Valor)
        };

        if (usuario.EmpresaId is not null)
        {
            claims.Add(new Claim("empresa_id", usuario.EmpresaId.Value.ToString()));
        }

        claims.AddRange(usuario.Papeis.Select(p => new Claim(ClaimTypes.Role, p.Papel.ToString())));

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.ChaveSecreta));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Emissor,
            audience: _settings.Audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutos),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Gera um refresh token opaco (não-JWT) criptograficamente aleatório.</summary>
    public string GerarRefreshTokenBruto() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <summary>
    /// RN10: o valor bruto do refresh token nunca é persistido - apenas seu
    /// hash SHA-256, evitando que um vazamento do banco exponha tokens
    /// utilizáveis diretamente.
    /// </summary>
    public string CalcularHash(string tokenBruto)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenBruto));
        return Convert.ToHexString(bytes);
    }

    public DateTime CalcularExpiracaoRefreshToken() => DateTime.UtcNow.AddDays(_settings.RefreshTokenDias);
}