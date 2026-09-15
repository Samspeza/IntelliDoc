using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IntelliDoc.Infrastructure.Identity;

/// <summary>
/// Implementação de IPasswordHasherService usando o IPasswordHasher do
/// ASP.NET Core Identity - ou seja, PBKDF2-HMAC-SHA256 com 100.000
/// iterações e salt por senha (padrão v3 do Identity), sem precisarmos
/// implementar criptografia por conta própria.
///
/// Nota: passamos `null!` como usuário ao chamar HashPassword porque a
/// implementação padrão do Identity não usa esse parâmetro - ele existe
/// apenas para permitir implementações customizadas que variem o hash por
/// usuário.
/// </summary>
public sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string GerarHash(string senhaPura) => _hasher.HashPassword(null!, senhaPura);

    public bool Verificar(string hashArmazenado, string senhaInformada)
    {
        var resultado = _hasher.VerifyHashedPassword(null!, hashArmazenado, senhaInformada);

        // SuccessRehashNeeded indica senha correta, porém com hash gerado
        // por uma versão mais antiga do algoritmo - tratamos como sucesso
        // (o rehash seria uma melhoria futura, registrada no roadmap).
        return resultado is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}