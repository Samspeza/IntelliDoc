using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Enums;
using IntelliDoc.Domain.Exceptions;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Aggregate Root que representa um evento de auditoria (RN32). Genérico por
/// design (EntidadeAfetada + EntidadeAfetadaId + snapshots JSON antes/depois)
/// para evitar uma tabela de auditoria por tipo de entidade, conforme
/// decisão registrada em docs/06-modelagem-banco.md (§4).
///
/// Não expõe NENHUM método de alteração após a criação (RN33 - imutável).
/// A Application nunca deve expor um Command de Update/Delete para este
/// agregado.
/// </summary>
public sealed class RegistroAuditoria : AggregateRoot
{
    public Guid? EmpresaId { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public TipoAcaoAuditoria Acao { get; private set; }
    public string EntidadeAfetada { get; private set; }
    public Guid? EntidadeAfetadaId { get; private set; }
    public string? DadosAntesJson { get; private set; }
    public string? DadosDepoisJson { get; private set; }
    public string EnderecoIp { get; private set; }

    private RegistroAuditoria()
    {
        EntidadeAfetada = string.Empty;
        EnderecoIp = string.Empty;
    }

    private RegistroAuditoria(
        Guid? empresaId,
        Guid? usuarioId,
        TipoAcaoAuditoria acao,
        string entidadeAfetada,
        Guid? entidadeAfetadaId,
        string? dadosAntesJson,
        string? dadosDepoisJson,
        string enderecoIp)
    {
        EmpresaId = empresaId;
        UsuarioId = usuarioId;
        Acao = acao;
        EntidadeAfetada = entidadeAfetada;
        EntidadeAfetadaId = entidadeAfetadaId;
        DadosAntesJson = dadosAntesJson;
        DadosDepoisJson = dadosDepoisJson;
        EnderecoIp = enderecoIp;
    }

    /// <summary>
    /// Único ponto de criação - chamado por um Behavior/Interceptor de
    /// auditoria na Application/Infrastructure sempre que uma ação sensível
    /// (RN32) é executada. EmpresaId nulo é aceito para ações de nível de
    /// plataforma (ex.: criação de uma nova empresa pelo SuperAdmin).
    /// </summary>
    public static RegistroAuditoria Criar(
        Guid? empresaId,
        Guid? usuarioId,
        TipoAcaoAuditoria acao,
        string entidadeAfetada,
        Guid? entidadeAfetadaId,
        string? dadosAntesJson,
        string? dadosDepoisJson,
        string enderecoIp)
    {
        if (string.IsNullOrWhiteSpace(entidadeAfetada))
        {
            throw new RegraDeNegocioException("AUDITORIA_ENTIDADE_OBRIGATORIA", "A entidade afetada é obrigatória.");
        }

        return new RegistroAuditoria(
            empresaId, usuarioId, acao, entidadeAfetada.Trim(), entidadeAfetadaId,
            dadosAntesJson, dadosDepoisJson, string.IsNullOrWhiteSpace(enderecoIp) ? "desconhecido" : enderecoIp);
    }
}