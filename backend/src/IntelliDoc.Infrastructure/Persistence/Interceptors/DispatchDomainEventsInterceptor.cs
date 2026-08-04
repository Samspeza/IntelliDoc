using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IntelliDoc.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Wrapper que adapta um IDomainEvent (definido no Domain, sem dependência
/// de MediatR - Etapa 9.1) para INotification (contrato do MediatR), usado
/// apenas dentro da Infrastructure. Handlers na Application escutam
/// DomainEventNotification&lt;DocumentoAprovadoEvent&gt;, por exemplo.
/// </summary>
public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}

/// <summary>
/// Despacha os eventos de domínio (DocumentoAprovadoEvent,
/// DocumentoRejeitadoEvent, etc.) acumulados em BaseEntity.DomainEvents
/// durante a execução de um Command (ex.: Documento.Aprovar adiciona
/// DocumentoAprovadoEvent). Roda em SavedChangesAsync (não em
/// SavingChangesAsync!) - ou seja, DEPOIS que a transação foi confirmada
/// no banco, garantindo que um handler de notificação (UC30) nunca reaja a
/// uma aprovação que acabou sendo revertida por outra falha na mesma
/// transação.
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await DespacharEventosAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DespacharEventosAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return;
        }

        var entidadesComEventos = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var eventos = entidadesComEventos
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entidade in entidadesComEventos)
        {
            entidade.LimparEventos();
        }

        foreach (var evento in eventos)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(evento.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, evento)!;
            await publisher.Publish(notification, cancellationToken);
        }
    }
}