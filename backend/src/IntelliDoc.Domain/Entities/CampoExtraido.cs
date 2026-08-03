using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Exceptions;
using IntelliDoc.Domain.ValueObjects;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Entidade filha do agregado Documento. Representa um campo extraído pela
/// IA/OCR (ex.: "ValorTotal", "DataEmissao") com seu confidence score
/// individual (RF09). Não possui repositório próprio - só é criada/corrigida
/// através do agregado Documento.
/// </summary>
public sealed class CampoExtraido : BaseEntity
{
    public Guid DocumentoId { get; private set; }
    public string NomeCampo { get; private set; }
    public string? ValorExtraidoIa { get; private set; }
    public string? ValorFinal { get; private set; }
    public ConfidenceScore Confidence { get; private set; } = null!;
    public bool CorrigidoManualmente { get; private set; }

    private CampoExtraido()
    {
        NomeCampo = string.Empty;
    }

    private CampoExtraido(Guid documentoId, string nomeCampo, string? valorExtraidoIa, ConfidenceScore confidence)
    {
        DocumentoId = documentoId;
        NomeCampo = nomeCampo;
        ValorExtraidoIa = valorExtraidoIa;
        ValorFinal = valorExtraidoIa; // por padrão, o valor final é o que a IA extraiu
        Confidence = confidence;
    }

    /// <summary>
    /// Criado pelo Worker (via Documento.RegistrarResultadoExtracao) a
    /// partir do resultado bruto do IDocumentExtractionService.
    /// </summary>
    public static CampoExtraido Criar(Guid documentoId, string nomeCampo, string? valorExtraidoIa, decimal confidenceScore)
    {
        if (string.IsNullOrWhiteSpace(nomeCampo))
        {
            throw new RegraDeNegocioException("CAMPO_NOME_OBRIGATORIO", "O nome do campo extraído é obrigatório.");
        }

        return new CampoExtraido(documentoId, nomeCampo.Trim(), valorExtraidoIa, ConfidenceScore.Criar(confidenceScore));
    }

    /// <summary>
    /// RF13/RN20: revisor corrige o valor. ValorExtraidoIa NUNCA é
    /// sobrescrito aqui - permanece como registro histórico do que a IA
    /// originalmente disse, usado futuramente para medir acurácia do modelo.
    /// </summary>
    public void Corrigir(string novoValor)
    {
        ValorFinal = novoValor;
        CorrigidoManualmente = true;
    }
}