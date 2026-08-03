using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Exceptions;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Aggregate Root que representa um tenant da plataforma (RN01-RN04,
/// docs/02-regras-de-negocio.md). Agrega ConfiguracaoEmpresa (1:1),
/// conforme docs/07-diagrama-entidades-dominio.md.
/// </summary>
public sealed class Empresa : AggregateRoot, IAuditavel
{
    public string Nome { get; private set; }
    public string? CnpjOuIdentificador { get; private set; }
    public bool Ativa { get; private set; }

    public ConfiguracaoEmpresa Configuracao { get; private set; } = null!;

    // Construtor privado exigido pelo EF Core (materialização via reflection).
    private Empresa()
    {
        Nome = string.Empty;
    }

    private Empresa(string nome, string? cnpjOuIdentificador)
    {
        Nome = nome;
        CnpjOuIdentificador = cnpjOuIdentificador;
        Ativa = true;
    }

    /// <summary>
    /// Cria uma nova empresa já com uma ConfiguracaoEmpresa padrão associada
    /// (limiar de confiança 70%, conforme RN36), evitando o estado
    /// intermediário "empresa sem configuração".
    /// </summary>
    public static Empresa Criar(string nome, string? cnpjOuIdentificador = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new RegraDeNegocioException("EMPRESA_NOME_OBRIGATORIO", "O nome da empresa é obrigatório.");
        }

        var empresa = new Empresa(nome.Trim(), cnpjOuIdentificador?.Trim());
        empresa.Configuracao = ConfiguracaoEmpresa.CriarPadrao(empresa.Id);
        return empresa;
    }

    /// <summary>
    /// RN03: ao desativar, bloqueia login de todos os usuários da empresa
    /// (a checagem efetiva de login é feita na Application, consultando
    /// Empresa.Ativa), mas preserva todos os dados.
    /// </summary>
    public void Desativar() => Ativa = false;

    public void Ativar() => Ativa = true;

    public void AtualizarDadosCadastrais(string nome, string? cnpjOuIdentificador)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new RegraDeNegocioException("EMPRESA_NOME_OBRIGATORIO", "O nome da empresa é obrigatório.");
        }

        Nome = nome.Trim();
        CnpjOuIdentificador = cnpjOuIdentificador?.Trim();
    }
}