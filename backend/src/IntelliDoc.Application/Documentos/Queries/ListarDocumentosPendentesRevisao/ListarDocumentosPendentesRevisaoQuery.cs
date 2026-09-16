using IntelliDoc.Application.Common.Models;
using IntelliDoc.Application.Documentos.Dtos;
using MediatR;

namespace IntelliDoc.Application.Documentos.Queries.ListarDocumentosPendentesRevisao;

/// <summary>UC19: fila de revisão, priorizando documentos de baixa confiança (RN16).</summary>
public sealed record ListarDocumentosPendentesRevisaoQuery(
    int Pagina = 1,
    int TamanhoPagina = 20) : IRequest<PaginatedList<DocumentoResumoDto>>;