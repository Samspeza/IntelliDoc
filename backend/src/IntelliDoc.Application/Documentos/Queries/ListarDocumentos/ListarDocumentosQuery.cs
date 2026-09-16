using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Documentos.Dtos;
using IntelliDoc.Domain.Enums;
using MediatR;

namespace IntelliDoc.Application.Documentos.Queries.ListarDocumentos;

/// <summary>
/// UC16: lista documentos com filtros (RF18). Todos os filtros são
/// opcionais; documentos arquivados são excluídos por padrão (RN17) a menos
/// que IncluirArquivados seja true.
/// </summary>
public sealed record ListarDocumentosQuery(
    StatusDocumento? Status = null,
    TipoDocumento? TipoDocumento = null,
    DateTime? PeriodoInicio = null,
    DateTime? PeriodoFim = null,
    bool IncluirArquivados = false,
    int Pagina = 1,
    int TamanhoPagina = 20) : IRequest<PaginatedList<DocumentoResumoDto>>;