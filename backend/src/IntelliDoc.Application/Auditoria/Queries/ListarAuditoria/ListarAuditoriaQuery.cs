using IntelliDoc.Application.Auditoria.Dtos;
using IntelliDoc.Application.Common.Models;
using IntelliDoc.Domain.Enums;
using MediatR;

namespace IntelliDoc.Application.Auditoria.Queries.ListarAuditoria;

/// <summary>UC33: consulta a trilha de auditoria (RN32/RN33), com filtros opcionais.</summary>
public sealed record ListarAuditoriaQuery(
    TipoAcaoAuditoria? Acao = null,
    DateTime? PeriodoInicio = null,
    DateTime? PeriodoFim = null,
    int Pagina = 1,
    int TamanhoPagina = 20) : IRequest<PaginatedList<RegistroAuditoriaDto>>;