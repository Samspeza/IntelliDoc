namespace IntelliDoc.Domain.Common;

/// <summary>
/// Interface de marcação usada pelo AuditableEntitySaveChangesInterceptor
/// (Infrastructure) para identificar quais entidades devem ter
/// CriadoEm/CriadoPor/AtualizadoEm/AtualizadoPor preenchidos automaticamente.
/// Como BaseEntity já expõe essas propriedades, esta interface serve apenas
/// como filtro explícito de "quero auditoria automática" no ChangeTracker do
/// EF Core, evitando acoplar o interceptor a "toda BaseEntity" por convenção
/// implícita.
/// </summary>
public interface IAuditavel
{
    DateTime CriadoEm { get; }
    string? CriadoPor { get; }
    DateTime? AtualizadoEm { get; }
    string? AtualizadoPor { get; }
}