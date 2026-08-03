using IntelliDoc.Domain.Enums;

namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai o acesso aos dados do usuário autenticado na requisição atual.
/// Implementada em Infrastructure.Identity.CurrentUserService, lendo as
/// claims do JWT (Etapa 4, §6). É a peça central da estratégia de
/// multi-tenant: EmpresaId aqui é o que o Global Query Filter do EF Core
/// usa para isolar dados entre tenants (RN02).
/// </summary>
public interface ICurrentUserService
{
    Guid? UsuarioId { get; }
    Guid? EmpresaId { get; }
    bool EstaAutenticado { get; }
    IReadOnlyCollection<PapelUsuario> Papeis { get; }
    string EnderecoIp { get; }

    bool TemPapel(PapelUsuario papel);
}