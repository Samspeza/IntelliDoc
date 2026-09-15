namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai o algoritmo de hashing de senha (RN09). Implementada por
/// Infrastructure.Identity.PasswordHasherService, que reaproveita o
/// IPasswordHasher&lt;T&gt; do ASP.NET Core Identity (PBKDF2 com as
/// configurações padrão recomendadas pela Microsoft) SEM exigir que a
/// entidade Usuario herde de IdentityUser - o que acoplaria o Domain ao
/// pacote Identity, violando a regra de dependência da Etapa 4.
/// </summary>
public interface IPasswordHasherService
{
    string GerarHash(string senhaPura);

    /// <summary>Retorna true se a senha informada corresponde ao hash armazenado.</summary>
    bool Verificar(string hashArmazenado, string senhaInformada);
}