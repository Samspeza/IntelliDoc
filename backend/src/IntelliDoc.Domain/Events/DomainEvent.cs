namespace IntelliDoc.Domain.Events;

/// <summary>
/// Contrato para eventos de domínio. Publicados via MediatR (Application)
/// pelo AuditableEntitySaveChangesInterceptor (Infrastructure) somente após
/// o SaveChangesAsync ter sucesso - assim, um handler de notificação nunca
/// reage a uma mudança que acabou não sendo persistida (ex.: por falha de
/// validação em outra parte da mesma transação).
/// </summary>
public interface IDomainEvent
{
    DateTime OcorridoEm { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OcorridoEm { get; } = DateTime.UtcNow;
}