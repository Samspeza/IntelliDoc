using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Enums;
using IntelliDoc.Domain.Exceptions;
using IntelliDoc.Domain.ValueObjects;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Aggregate Root que representa um usuário da plataforma. Agrega
/// UsuarioPapel (N papéis por usuário, RN06) e RefreshToken (N sessões
/// ativas), conforme docs/07-diagrama-entidades-dominio.md.
/// EmpresaId é nulo apenas para o SuperAdmin da plataforma (RN01).
/// </summary>
public sealed class Usuario : AggregateRoot, IAuditavel
{
    public Guid? EmpresaId { get; private set; }
    public string Nome { get; private set; }
    public Email Email { get; private set; } = null!;
    public string SenhaHash { get; private set; }
    public bool Ativo { get; private set; }

    private readonly List<UsuarioPapel> _papeis = [];
    public IReadOnlyCollection<UsuarioPapel> Papeis => _papeis.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private Usuario()
    {
        Nome = string.Empty;
        SenhaHash = string.Empty;
    }

    private Usuario(Guid? empresaId, string nome, Email email, string senhaHash)
    {
        EmpresaId = empresaId;
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Ativo = true;
    }

    /// <summary>
    /// Cria um usuário comum, vinculado a uma empresa. Para o SuperAdmin da
    /// plataforma, usar CriarSuperAdmin. O usuário é criado sem papéis -
    /// ao menos um papel deve ser adicionado via AdicionarPapel antes de
    /// persistir (validado pelo Command Handler correspondente, ex.:
    /// CriarUsuarioCommandHandler).
    /// </summary>
    public static Usuario Criar(Guid empresaId, string nome, string email, string senhaHash)
    {
        ValidarNomeESenha(nome, senhaHash);
        return new Usuario(empresaId, nome.Trim(), Email.Criar(email), senhaHash);
    }

    public static Usuario CriarSuperAdmin(string nome, string email, string senhaHash)
    {
        ValidarNomeESenha(nome, senhaHash);
        var usuario = new Usuario(empresaId: null, nome.Trim(), Email.Criar(email), senhaHash);
        usuario.AdicionarPapel(PapelUsuario.SuperAdmin);
        return usuario;
    }

    private static void ValidarNomeESenha(string nome, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new RegraDeNegocioException("USUARIO_NOME_OBRIGATORIO", "O nome do usuário é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(senhaHash))
        {
            throw new RegraDeNegocioException("USUARIO_SENHA_OBRIGATORIA", "O hash de senha não pode ser vazio.");
        }
    }

    /// <summary>RN06: um usuário pode ter mais de um papel na mesma empresa.</summary>
    public void AdicionarPapel(PapelUsuario papel)
    {
        if (_papeis.Any(p => p.Papel == papel))
        {
            return; // idempotente - já possui o papel
        }

        _papeis.Add(UsuarioPapel.Criar(Id, papel));
    }

    public void RemoverPapel(PapelUsuario papel)
    {
        var existente = _papeis.FirstOrDefault(p => p.Papel == papel);
        if (existente is not null)
        {
            _papeis.Remove(existente);
        }
    }

    public bool TemPapel(PapelUsuario papel) => _papeis.Any(p => p.Papel == papel);

    /// <summary>RN08: usuário desativado não autentica, mas histórico é preservado.</summary>
    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    public void AtualizarSenha(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash))
        {
            throw new RegraDeNegocioException("USUARIO_SENHA_OBRIGATORIA", "O hash de senha não pode ser vazio.");
        }

        SenhaHash = novaSenhaHash;

        // RN10: troca de senha invalida todos os refresh tokens ativos.
        foreach (var token in _refreshTokens.Where(t => t.EstaValido()))
        {
            token.Revogar();
        }
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new RegraDeNegocioException("USUARIO_NOME_OBRIGATORIO", "O nome do usuário é obrigatório.");
        }

        Nome = nome.Trim();
    }

    /// <summary>Emite um novo refresh token (sessão), conforme RN10.</summary>
    public RefreshToken EmitirRefreshToken(string tokenHash, DateTime expiraEm)
    {
        var token = RefreshToken.Criar(Id, tokenHash, expiraEm);
        _refreshTokens.Add(token);
        return token;
    }

    /// <summary>RN10: logout invalida o refresh token corrente.</summary>
    public void RevogarRefreshToken(string tokenHash)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        token?.Revogar();
    }
}