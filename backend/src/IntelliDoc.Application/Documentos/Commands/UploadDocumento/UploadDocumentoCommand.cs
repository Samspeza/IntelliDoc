using MediatR;

namespace IntelliDoc.Application.Documentos.Commands.UploadDocumento;

/// <summary>
/// UC13 (docs/03-casos-de-uso.md): Operador faz upload de um documento.
/// O Stream do arquivo é passado diretamente (não persistido em memória
/// como byte[]) para suportar arquivos maiores sem pressão excessiva de
/// memória - RN11 limita a 10MB, mas a decisão de usar Stream generaliza
/// bem caso o limite mude no futuro.
/// Retorna apenas o Id do documento criado (RF11 - o status é consultado
/// separadamente via ObterDocumentoPorId, mantendo o Command enxuto).
/// </summary>
public sealed record UploadDocumentoCommand(
    Stream ConteudoArquivo,
    string NomeArquivoOriginal,
    string TipoArquivo,
    long TamanhoBytes) : IRequest<Guid>;