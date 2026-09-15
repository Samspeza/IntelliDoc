using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Identidade.Commands.Login;

/// <summary>
/// Executa o UC02. Checagens em ordem: usuário existe → senha confere →
/// usuário ativo (RN08) → empresa ativa (RN03).
///
/// Decisão de segurança: TODAS as falhas retornam a mesma
/// ForbiddenAccessException com mensagem genérica ("Credenciais
/// inválidas"), sem distinguir "e-mail não existe" de "senha errada" - caso
/// contrário, o endpoint viraria um oráculo para enumerar quais e-mails
/// estão cadastrados na plataforma.
/// </summary>
public sealed class LoginCommandHandler(
    IApplicationDbContext dbContext,
    IPasswordHasherService passwordHasher,
    ITokenService tokenService,
    ICurrentUserService currentUser)
    : IRequestHandler<LoginCommand, RespostaAutenticacao>
{
    private const string MensagemGenerica = "Credenciais inválidas.";

    public async Task<RespostaAutenticacao> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        // IgnoreQueryFilters: no login ainda não há tenant resolvido
        // (o JWT é justamente o que estamos emitindo).
        var usuario = await dbContext.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.Papeis)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.Valor == emailNormalizado, cancellationToken);

        if (usuario is null || !passwordHasher.Verificar(usuario.SenhaHash, request.Senha))
        {
            throw new ForbiddenAccessException(MensagemGenerica);
        }

        if (!usuario.Ativo)
        {
            throw new ForbiddenAccessException(MensagemGenerica); // RN08
        }

        if (usuario.EmpresaId is not null)
        {
            var empresaAtiva = await dbContext.Empresas
                .IgnoreQueryFilters()
                .AnyAsync(e => e.Id == usuario.EmpresaId && e.Ativa, cancellationToken);

            if (!empresaAtiva)
            {
                throw new ForbiddenAccessException(MensagemGenerica); // RN03
            }
        }

        var tokens = tokenService.EmitirTokens(usuario);

        var novoRefreshToken = usuario.EmitirRefreshToken(
            tokens.RefreshTokenHash,
            tokens.RefreshTokenExpiraEm);

        Console.WriteLine($"[DEBUG] NOVO RefreshToken: {novoRefreshToken.Id}");

        if (dbContext is DbContext context)
        {
            Console.WriteLine(
                $"[DEBUG] Estado do NOVO RefreshToken: {context.Entry(novoRefreshToken).State}");
        }

        dbContext.RegistrosAuditoria.Add(RegistroAuditoria.Criar(
            empresaId: usuario.EmpresaId,
            usuarioId: usuario.Id,
            acao: TipoAcaoAuditoria.Login,
            entidadeAfetada: nameof(Usuario),
            entidadeAfetadaId: usuario.Id,
            dadosAntesJson: null,
            dadosDepoisJson: null,
            enderecoIp: currentUser.EnderecoIp));

        var estadosRefreshTokens = dbContext.Usuarios
            .Local
            .SelectMany(u => u.RefreshTokens)
            .Select(t => new
            {
                t.Id,
                Estado = dbContext is DbContext context
                    ? context.Entry(t).State.ToString()
                    : "N/A"
            })
            .ToList();

        foreach (var item in estadosRefreshTokens)
        {
            Console.WriteLine($"[DEBUG] RefreshToken {item.Id} -> {item.Estado}");
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RespostaAutenticacao(
            tokens.AccessToken,
            tokens.RefreshTokenBruto,
            tokens.AccessTokenExpiraEm,
            usuario.Id,
            usuario.Nome,
            usuario.Email.Valor,
            usuario.EmpresaId,
            usuario.Papeis.Select(p => p.Papel.ToString()).ToList());
    }
}