using MediatR;

namespace IntelliDoc.Application.Documentos.Commands.AprovarDocumento;

/// <summary>UC22: revisor/gestor aprova o documento.</summary>
public sealed record AprovarDocumentoCommand(Guid DocumentoId) : IRequest;