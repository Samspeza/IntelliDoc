using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Notificacoes.EventHandlers;

/// <summary>
/// UC30: reage a DocumentoAprovadoEvent (disparado por Documento.Aprovar,
/// Etapa 9.2, e publicado pelo DispatchDomainEventsInterceptor, Etapa 9.4)
/// criando uma Notificacao in-app e disparando um e-mail ao operador que
/// enviou o documento. Este handler é a prova de que o desacoplamento
/// funciona: Documento não importa nada deste arquivo.
/// </summary>
public sealed class NotificarDocumentoAprovadoEventHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService)
    : INotificationHandler<DomainEventNotification<DocumentoAprovadoEvent>>
{
    public async Task Handle(DomainEventNotification<DocumentoAprovadoEvent> notification, CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        var operador = await dbContext.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == evento.EnviadoPorUsuarioId, cancellationToken);

        if (operador is null)
        {
            return;
        }

        var notificacao = Notificacao.CriarParaUsuario(
            evento.EmpresaId,
            evento.EnviadoPorUsuarioId,
            titulo: "Documento aprovado",
            mensagem: $"Seu documento \"{evento.NomeArquivoOriginal}\" foi aprovado.",
            documentoRelacionadoId: evento.DocumentoId);

        dbContext.Notificacoes.Add(notificacao);
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailService.EnviarAsync(
            operador.Email.Valor,
            "Seu documento foi aprovado",
            $"<p>Olá {operador.Nome},</p><p>Seu documento <strong>{evento.NomeArquivoOriginal}</strong> foi aprovado.</p>",
            cancellationToken);
    }
}