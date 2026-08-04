using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IntelliDoc.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Preenche automaticamente as colunas de auditoria padrão (CriadoEm,
/// CriadoPor, AtualizadoEm, AtualizadoPor - docs/06-modelagem-banco.md §1)
/// para toda entidade que implementa IAuditavel, a cada SaveChanges[Async].
/// Isso elimina a necessidade de qualquer Command Handler lembrar de
/// preencher esses campos manualmente - é responsabilidade transversal de
/// persistência, não de caso de uso.
/// </summary>
public sealed class AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AtualizarEntidadesAuditaveis(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AtualizarEntidadesAuditaveis(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AtualizarEntidadesAuditaveis(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var usuarioAtual = currentUser.UsuarioId?.ToString() ?? "sistema";

        foreach (EntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DefinirCriacao(usuarioAtual);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.DefinirAtualizacao(usuarioAtual);
            }
        }
    }
}