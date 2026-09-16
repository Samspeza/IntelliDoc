using IntelliDoc.Application.Dashboard.Dtos;
using MediatR;

namespace IntelliDoc.Application.Dashboard.Queries.ObterIndicadores;

/// <summary>UC25/UC26: indicadores do dashboard, com filtro de período opcional.</summary>
public sealed record ObterIndicadoresQuery(
    DateTime? PeriodoInicio = null,
    DateTime? PeriodoFim = null) : IRequest<IndicadoresDashboardDto>;