using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Identidade.Commands.RefreshToken;

/// <summary>
/// Executa o UC03 com ROTAÇÃO de refresh token: o token apresentado é
/// revogado e um novo é emitido a cada renovação. Isso limita a janela de
/// uso de um token vazado - se o atacante usar o token antes do usuário
/// legítimo, a próxima tentativa do usuário falhará (sinal detectável); se
/// usar depois, o token já estará revogado.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext dbContext,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, RespostaAutenticacao>
{
    public async Task<RespostaAutenticacao> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashInformado = tokenService.CalcularHashRefreshToken(request.RefreshToken);

        var usuario = await dbContext.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.Papeis)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == hashInformado), cancellationToken)
            ?? throw new ForbiddenAccessException("Refresh token inválido.");

        var tokenArmazenado = usuario.RefreshTokens.First(t => t.TokenHash == hashInformado);

        if (!tokenArmazenado.EstaValido())
        {
            throw new ForbiddenAccessException("Refresh token expirado ou revogado.");
        }

        if (!usuario.Ativo)
        {
            throw new ForbiddenAccessException("Usuário inativo."); // RN08
        }

        // Rotação: revoga o token usado antes de emitir o novo.
        tokenArmazenado.Revogar();

        var novosTokens = tokenService.EmitirTokens(usuario);
        usuario.EmitirRefreshToken(novosTokens.RefreshTokenHash, novosTokens.RefreshTokenExpiraEm);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RespostaAutenticacao(
            novosTokens.AccessToken,
            novosTokens.RefreshTokenBruto,
            novosTokens.AccessTokenExpiraEm,
            usuario.Id,
            usuario.Nome,
            usuario.Email.Valor,
            usuario.EmpresaId,
            usuario.Papeis.Select(p => p.Papel.ToString()).ToList());
    }
}