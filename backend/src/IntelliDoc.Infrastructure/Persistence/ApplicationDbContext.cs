using System.Reflection;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Infrastructure.Persistence;

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
        modelBuilder.Entity<Documento>()
            .HasQueryFilter(d => currentUser.EmpresaId == null || d.EmpresaId == currentUser.EmpresaId);

        modelBuilder.Entity<Notificacao>()
            .HasQueryFilter(n => currentUser.EmpresaId == null || n.EmpresaId == currentUser.EmpresaId);

        modelBuilder.Entity<RegistroAuditoria>()
            .HasQueryFilter(a => currentUser.EmpresaId == null || a.EmpresaId == currentUser.EmpresaId);

        modelBuilder.Entity<Usuario>()
            .HasQueryFilter(u => currentUser.EmpresaId == null || u.EmpresaId == currentUser.EmpresaId);

        // IDs são gerados pela aplicação com Guid.NewGuid().
        // Portanto, o EF Core não deve tratá-los como valores
        // gerados pelo banco.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));

            if (idProperty is not null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated =
                    Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}