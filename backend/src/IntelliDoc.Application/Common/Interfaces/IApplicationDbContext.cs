using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstração do ApplicationDbContext (EF Core), permitindo que Commands e
/// Queries usem DbSet&lt;T&gt; sem a Application referenciar o pacote
/// Microsoft.EntityFrameworkCore diretamente por conta própria além do
/// necessário para tipar DbSet/SaveChangesAsync - a implementação concreta
/// (com toda a configuração de mapeamento, interceptors, Global Query
/// Filter de multi-tenant) fica em IntelliDoc.Infrastructure.Persistence.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Empresa> Empresas { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Documento> Documentos { get; }
    DbSet<Notificacao> Notificacoes { get; }
    DbSet<RegistroAuditoria> RegistrosAuditoria { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}