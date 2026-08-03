using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Enums;
using IntelliDoc.Domain.Exceptions;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Entidade filha do agregado Empresa (docs/07-diagrama-entidades-dominio.md).
/// Não possui repositório próprio: só é criada/alterada através de Empresa.
/// Representa as configurações por empresa descritas em RN36.
/// </summary>
public sealed class ConfiguracaoEmpresa : BaseEntity
{
    public Guid EmpresaId { get; private set; }
    public decimal LimiarConfiancaIa { get; private set; }
    public List<TipoDocumento> TiposDocumentoAceitos { get; private set; } = [];
    public bool SegregacaoRevisorAtiva { get; private set; }

    private ConfiguracaoEmpresa()
    {
    }

    private ConfiguracaoEmpresa(Guid empresaId, decimal limiarConfiancaIa, List<TipoDocumento> tiposAceitos, bool segregacaoAtiva)
    {
        EmpresaId = empresaId;
        LimiarConfiancaIa = limiarConfiancaIa;
        TiposDocumentoAceitos = tiposAceitos;
        SegregacaoRevisorAtiva = segregacaoAtiva;
    }

    /// <summary>
    /// Configuração padrão aplicada a toda empresa recém-criada: limiar de
    /// 70% (RN16), todos os tipos de documento aceitos, segregação
    /// desabilitada (RN24 é opt-in).
    /// </summary>
    public static ConfiguracaoEmpresa CriarPadrao(Guid empresaId) =>
        new(
            empresaId,
            limiarConfiancaIa: 70.00m,
            tiposAceitos: [TipoDocumento.NotaFiscal, TipoDocumento.Recibo, TipoDocumento.Contrato, TipoDocumento.Outro],
            segregacaoAtiva: false);

    /// <summary>
    /// RN37: alterar configuração é ação auditada - a auditoria em si é
    /// responsabilidade do Command Handler (Application), este método apenas
    /// garante a validação de domínio (limiar dentro do intervalo válido).
    /// </summary>
    public void Atualizar(decimal limiarConfiancaIa, List<TipoDocumento> tiposDocumentoAceitos, bool segregacaoRevisorAtiva)
    {
        if (limiarConfiancaIa is < 0 or > 100)
        {
            throw new RegraDeNegocioException(
                "LIMIAR_CONFIANCA_INVALIDO",
                "O limiar de confiança deve estar entre 0 e 100.");
        }

        if (tiposDocumentoAceitos.Count == 0)
        {
            throw new RegraDeNegocioException(
                "TIPOS_DOCUMENTO_VAZIO",
                "É necessário aceitar ao menos um tipo de documento.");
        }

        LimiarConfiancaIa = limiarConfiancaIa;
        TiposDocumentoAceitos = tiposDocumentoAceitos;
        SegregacaoRevisorAtiva = segregacaoRevisorAtiva;
    }
}