using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Notificacoes.EventHandlers;

/// <summary>Análogo a NotificarDocumentoAprovadoEventHandler, incluindo o motivo (RN21) na mensagem.</summary>
public sealed class NotificarDocumentoRejeitadoEventHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService)
    : INotificationHandler<DomainEventNotification<DocumentoRejeitadoEvent>>
{
    public async Task Handle(DomainEventNotification<DocumentoRejeitadoEvent> notification, CancellationToken cancellationToken)
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
            titulo: "Documento rejeitado",
            mensagem: $"Seu documento \"{evento.NomeArquivoOriginal}\" foi rejeitado: {evento.Motivo}",
            documentoRelacionadoId: evento.DocumentoId);

        dbContext.Notificacoes.Add(notificacao);
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailService.EnviarAsync(
            operador.Email.Valor,
            "Seu documento foi rejeitado",
            $"<p>Olá {operador.Nome},</p><p>Seu documento <strong>{evento.NomeArquivoOriginal}</strong> foi rejeitado.</p><p><em>Motivo: {evento.Motivo}</em></p>",
            cancellationToken);
    }
}