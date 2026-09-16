using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Notificacoes.Commands.MarcarNotificacaoComoLida;

/// <summary>
/// Executa a marcação de leitura. Valida explicitamente que a notificação
/// pertence ao usuário (individual) ou a um papel que ele possui (agregada)
/// - sem essa checagem, um usuário poderia marcar como lida uma notificação
/// de outro usuário da mesma empresa apenas adivinhando o Id (o Global
/// Query Filter isola por EMPRESA, não por usuário individual).
/// </summary>
public sealed class MarcarNotificacaoComoLidaCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<MarcarNotificacaoComoLidaCommand>
{
    public async Task Handle(MarcarNotificacaoComoLidaCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UsuarioId!.Value;

        var notificacao = await dbContext.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == request.NotificacaoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notificacao), request.NotificacaoId);

        var pertenceAoUsuario = notificacao.UsuarioDestinoId == usuarioId
            || (notificacao.PapelDestino is not null && currentUser.TemPapel(notificacao.PapelDestino.Value));

        if (!pertenceAoUsuario)
        {
            throw new ForbiddenAccessException("Esta notificação não pertence ao usuário autenticado.");
        }

        notificacao.MarcarComoLida();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}