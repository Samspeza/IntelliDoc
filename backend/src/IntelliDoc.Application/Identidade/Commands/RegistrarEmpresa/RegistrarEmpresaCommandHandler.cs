using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Enums;
using IntelliDoc.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Identidade.Commands.RegistrarEmpresa;

/// <summary>
/// Executa o UC01. Cria Empresa + primeiro Usuario (AdminEmpresa) em uma
/// única unidade de trabalho, e registra auditoria da criação da empresa
/// (RN32, TipoAcaoAuditoria.CriarEmpresa).
///
/// Nota sobre o Global Query Filter: a verificação de e-mail duplicado usa
/// IgnoreQueryFilters porque, neste ponto, ainda não há usuário autenticado
/// - o filtro de tenant retornaria zero linhas e permitiria criar dois
/// usuários com o mesmo e-mail em empresas diferentes, violando a
/// constraint UNIQUE de "Usuarios"."Email" (Etapa 8) e estourando um erro
/// de banco pouco amigável em vez de uma mensagem clara.
/// </summary>
public sealed class RegistrarEmpresaCommandHandler(
    IApplicationDbContext dbContext,
    IPasswordHasherService passwordHasher,
    ITokenService tokenService,
    ICurrentUserService currentUser)
    : IRequestHandler<RegistrarEmpresaCommand, RespostaAutenticacao>
{
    public async Task<RespostaAutenticacao> Handle(RegistrarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var emailJaExiste = await dbContext.Usuarios
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.Valor == emailNormalizado, cancellationToken);

        if (emailJaExiste)
        {
            throw new RegraDeNegocioException("EMAIL_JA_CADASTRADO", "Já existe um usuário cadastrado com este e-mail.");
        }

        var empresa = Empresa.Criar(request.NomeEmpresa, request.CnpjOuIdentificador);

        var senhaHash = passwordHasher.GerarHash(request.Senha);
        var administrador = Usuario.Criar(empresa.Id, request.NomeAdministrador, emailNormalizado, senhaHash);
        administrador.AdicionarPapel(PapelUsuario.AdminEmpresa);

        var tokens = tokenService.EmitirTokens(administrador);
        administrador.EmitirRefreshToken(tokens.RefreshTokenHash, tokens.RefreshTokenExpiraEm);

        var auditoria = RegistroAuditoria.Criar(
            empresaId: empresa.Id,
            usuarioId: administrador.Id,
            acao: TipoAcaoAuditoria.CriarEmpresa,
            entidadeAfetada: nameof(Empresa),
            entidadeAfetadaId: empresa.Id,
            dadosAntesJson: null,
            dadosDepoisJson: $$"""{"nome":"{{empresa.Nome}}"}""",
            enderecoIp: currentUser.EnderecoIp);

        dbContext.Empresas.Add(empresa);
        dbContext.Usuarios.Add(administrador);
        dbContext.RegistrosAuditoria.Add(auditoria);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RespostaAutenticacao(
            tokens.AccessToken,
            tokens.RefreshTokenBruto,
            tokens.AccessTokenExpiraEm,
            administrador.Id,
            administrador.Nome,
            administrador.Email.Valor,
            administrador.EmpresaId,
            administrador.Papeis.Select(p => p.Papel.ToString()).ToList());
    }
}