using IntelliDoc.Domain.Events;
using MediatR;

namespace IntelliDoc.Application.Common.Models;

/// <summary>
/// Adapta um IDomainEvent (definido no Domain, sem dependência de MediatR -
/// Etapa 9.1) para INotification (contrato do MediatR). Publicado pelo
/// DispatchDomainEventsInterceptor (Infrastructure, Etapa 9.4) após
/// SaveChangesAsync ter sucesso, e consumido por handlers na Application
/// como NotificarDocumentoAprovadoEventHandler (Etapa 9.10).
///
/// CORREÇÃO (Etapa 9.10): esta classe estava originalmente definida em
/// Infrastructure.Persistence.Interceptors (Etapa 9.4), mas isso obrigaria
/// handlers da Application a referenciar Infrastructure - violando a regra
/// de dependência da Clean Architecture (Etapa 4: Infrastructure depende de
/// Application, nunca o contrário). Ela pertence à Application, que já
/// depende do pacote MediatR.
/// </summary>
public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}