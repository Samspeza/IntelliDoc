using IntelliDoc.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Identidade.Commands.Logout;

/// <summary>
/// Executa o UC06. Não lança exceção se o token não for encontrado ou já
/// estiver revogado - logout é idempotente por natureza, e retornar erro
/// aqui só criaria ruído no cliente sem ganho de segurança.
/// </summary>
public sealed class LogoutCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser,
    ITokenService tokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UsuarioId is null)
        {
            return;
        }

        var usuario = await dbContext.Usuarios
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == currentUser.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return;
        }

        usuario.RevogarRefreshToken(tokenService.CalcularHashRefreshToken(request.RefreshToken));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}