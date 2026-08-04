using System.Reflection;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Infrastructure.Persistence;

/// <summary>
/// Implementação concreta de IApplicationDbContext (Application, Etapa 9.3).
/// Duas responsabilidades centrais além do mapeamento padrão do EF Core:
///
/// 1. Global Query Filter de multi-tenant (RN02): aplicado em
///    OnModelCreating a toda entidade com EmpresaId, filtrando
///    automaticamente por ICurrentUserService.EmpresaId. SuperAdmin
///    (EmpresaId nulo) não tem filtro aplicado - enxerga todas as empresas,
///    conforme UC12.
/// 2. Carregamento de todas as IEntityTypeConfiguration via reflection
///    (ApplyConfigurationsFromAssembly), mantendo o mapeamento de cada
///    entidade isolado em seu próprio arquivo (Configurations/).
/// </summary>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUser)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // --- Global Query Filter de multi-tenant (RN02) ---
        // SuperAdmin (currentUser.EmpresaId == null e usuário é SuperAdmin)
        // não é filtrado - ver ICurrentUserService.EmpresaId, que retorna
        // null tanto para "não autenticado" quanto para "SuperAdmin"; a
        // distinção entre os dois casos é feita na Application (Queries do
        // módulo Empresas exigem explicitamente o papel SuperAdmin antes de
        // consultar sem filtro).
        modelBuilder.Entity<Documento>().HasQueryFilter(d => currentUser.EmpresaId == null || d.EmpresaId == currentUser.EmpresaId);
        modelBuilder.Entity<Notificacao>().HasQueryFilter(n => currentUser.EmpresaId == null || n.EmpresaId == currentUser.EmpresaId);
        modelBuilder.Entity<RegistroAuditoria>().HasQueryFilter(a => currentUser.EmpresaId == null || a.EmpresaId == currentUser.EmpresaId);
        modelBuilder.Entity<Usuario>().HasQueryFilter(u => currentUser.EmpresaId == null || u.EmpresaId == currentUser.EmpresaId);

        base.OnModelCreating(modelBuilder);
    }
}