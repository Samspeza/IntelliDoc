using AutoMapper;
using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Application.Documentos.Dtos;
using IntelliDoc.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Documentos.Queries.ObterDocumentoPorId;

/// <summary>
/// Executa o UC15. Não há checagem explícita de tenant aqui: o Global Query
/// Filter (Etapa 9.4) já restringe a consulta à empresa do usuário
/// autenticado. Se o documento pertencer a outra empresa, a query retorna
/// vazio e lançamos NotFoundException - deliberadamente o mesmo erro de
/// "não existe", para não revelar que o recurso existe em outro tenant
/// (RN02).
///
/// AsNoTracking: é uma leitura pura, não precisamos do change tracker -
/// reduz alocação e tempo de materialização.
/// </summary>
public sealed class ObterDocumentoPorIdQueryHandler(
    IApplicationDbContext dbContext,
    IMapper mapper) : IRequestHandler<ObterDocumentoPorIdQuery, DocumentoDetalheDto>
{
    public async Task<DocumentoDetalheDto> Handle(ObterDocumentoPorIdQuery request, CancellationToken cancellationToken)
    {
        var documento = await dbContext.Documentos
            .AsNoTracking()
            .Include(d => d.Campos)
            .Include(d => d.Historico)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Documento), request.DocumentoId);

        return mapper.Map<DocumentoDetalheDto>(documento);
    }
}