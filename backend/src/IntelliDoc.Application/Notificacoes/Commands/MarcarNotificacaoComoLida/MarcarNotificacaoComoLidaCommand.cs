using MediatR;

namespace IntelliDoc.Application.Notificacoes.Commands.MarcarNotificacaoComoLida;

public sealed record MarcarNotificacaoComoLidaCommand(Guid NotificacaoId) : IRequest;