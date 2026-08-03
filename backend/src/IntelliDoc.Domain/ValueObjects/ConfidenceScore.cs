using IntelliDoc.Domain.Exceptions;

namespace IntelliDoc.Domain.ValueObjects;

/// <summary>
/// Value Object que representa o grau de confiança (0 a 100) da extração
/// de IA/OCR para um campo ou para um documento inteiro. Referenciado em
/// docs/07-diagrama-entidades-dominio.md e usado pelas regras RN15/RN16
/// (priorização de revisão quando o score fica abaixo do limiar da
/// empresa).
/// </summary>
public sealed record ConfidenceScore
{
    public decimal Valor { get; }

    private ConfidenceScore(decimal valor)
    {
        Valor = valor;
    }

    public static ConfidenceScore Criar(decimal valor)
    {
        if (valor < 0 || valor > 100)
        {
            throw new RegraDeNegocioException(
                "CONFIDENCE_SCORE_FORA_DO_INTERVALO",
                $"O confidence score deve estar entre 0 e 100. Valor recebido: {valor}.");
        }

        return new ConfidenceScore(valor);
    }

    /// <summary>
    /// RN16: documento é marcado como PrioridadeRevisao quando o score médio
    /// fica abaixo do limiar configurado pela empresa (ConfiguracaoEmpresa).
    /// </summary>
    public bool EstaAbaixoDoLimiar(decimal limiar) => Valor < limiar;

    /// <summary>
    /// Calcula o score médio de uma coleção de scores (usado para consolidar
    /// o ConfidenceScoreMedio do Documento a partir dos scores individuais
    /// de cada CampoExtraido).
    /// </summary>
    public static ConfidenceScore CalcularMedia(IEnumerable<ConfidenceScore> scores)
    {
        var lista = scores.ToList();

        if (lista.Count == 0)
        {
            throw new RegraDeNegocioException(
                "CONFIDENCE_SCORE_SEM_CAMPOS",
                "Não é possível calcular a média de confiança sem nenhum campo extraído.");
        }

        var media = lista.Average(s => s.Valor);
        return Criar(Math.Round(media, 2));
    }

    public override string ToString() => $"{Valor:0.00}%";
}