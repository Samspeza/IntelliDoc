using MediatR;

namespace IntelliDoc.Application.Documentos.Commands.RejeitarDocumento;

/// <summary>UC23: revisor/gestor rejeita o documento, com motivo obrigatório (RN21).</summary>
public sealed record RejeitarDocumentoCommand(Guid DocumentoId, string Motivo) : IRequest;