using System.Security.Claims;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace IntelliDoc.Infrastructure.Identity;

/// <summary>
/// Implementação de ICurrentUserService (Application, Etapa 9.3). Lê as
/// claims do JWT já validado pelo middleware de autenticação do ASP.NET
/// Core (registrado em DependencyInjection.cs), extraindo:
///   - "sub"        -> UsuarioId
///   - "empresa_id"  -> EmpresaId (claim customizada, ausente para SuperAdmin)
///   - "role"        -> Papeis (pode haver múltiplas claims "role", RN06)
/// É esta classe que alimenta o Global Query Filter de multi-tenant
/// (ApplicationDbContext, Etapa 9.4) - nenhuma outra parte do sistema lê
/// claims diretamente do HttpContext.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Usuario => httpContextAccessor.HttpContext?.User;

    public bool EstaAutenticado => Usuario?.Identity?.IsAuthenticated ?? false;

    public Guid? UsuarioId
    {
        get
        {
            var valor = Usuario?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public Guid? EmpresaId
    {
        get
        {
            var valor = Usuario?.FindFirstValue("empresa_id");
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public IReadOnlyCollection<PapelUsuario> Papeis =>
        Usuario?.FindAll(ClaimTypes.Role)
            .Select(c => Enum.TryParse<PapelUsuario>(c.Value, out var papel) ? papel : (PapelUsuario?)null)
            .Where(p => p is not null)
            .Select(p => p!.Value)
            .ToList()
        ?? [];

    public string EnderecoIp =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

    public bool TemPapel(PapelUsuario papel) => Papeis.Contains(papel);
}