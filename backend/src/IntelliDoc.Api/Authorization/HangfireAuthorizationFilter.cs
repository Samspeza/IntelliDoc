using Hangfire.Dashboard;
using IntelliDoc.Domain.Enums;

namespace IntelliDoc.Api.Authorization;

/// <summary>
/// Protege o dashboard do Hangfire (/hangfire). Por padrão, o dashboard é
/// PÚBLICO quando exposto - o que exporia dados de jobs (incluindo Ids de
/// documentos e empresas) a qualquer visitante. Este filtro exige usuário
/// autenticado com papel SuperAdmin ou AdminEmpresa (Etapa 4, §7).
/// </summary>
public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        return httpContext.User.IsInRole(nameof(PapelUsuario.SuperAdmin))
            || httpContext.User.IsInRole(nameof(PapelUsuario.AdminEmpresa));
    }
}