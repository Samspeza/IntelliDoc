using IntelliDoc.Application.Documentos.Dtos;
using MediatR;

namespace IntelliDoc.Application.Documentos.Queries.ObterDocumentoPorId;

/// <summary>UC15: consulta o detalhe completo de um documento.</summary>
public sealed record ObterDocumentoPorIdQuery(Guid DocumentoId) : IRequest<DocumentoDetalheDto>;