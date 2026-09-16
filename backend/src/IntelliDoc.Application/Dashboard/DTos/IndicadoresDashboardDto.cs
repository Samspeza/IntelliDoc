namespace IntelliDoc.Application.Dashboard.Dtos;

/// <summary>Agregados exibidos no dashboard (RF17). Calculado sobre o período filtrado.</summary>
public sealed record IndicadoresDashboardDto(
    int TotalProcessados,
    int TotalAprovados,
    int TotalRejeitados,
    int TotalPendentesRevisao,
    double TaxaAprovacaoPercentual, // RN26
    double TempoMedioProcessamentoMinutos,
    IReadOnlyCollection<VolumePorTipoDto> VolumePorTipo);

public sealed record VolumePorTipoDto(string TipoDocumento, int Quantidade);