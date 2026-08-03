using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using MediatR;

namespace IntelliDoc.Application.Documentos.Commands.UploadDocumento;

/// <summary>
/// Orquestra o UC13: salva o arquivo no storage (IFileStorageService), cria
/// o agregado Documento (RN11/RN12/RN13, status inicial Enviado), persiste,
/// e só então enfileira o processamento (IDocumentProcessingQueue) - nessa
/// ordem, para nunca enfileirar um job apontando para um documento que não
/// foi persistido com sucesso.
/// EmpresaId e EnviadoPorUsuarioId vêm de ICurrentUserService (claims do
/// JWT), nunca do próprio Command - eliminando a possibilidade de um
/// usuário forjar o upload em nome de outra empresa (RN02/RN04).
/// </summary>
public sealed class UploadDocumentoCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser,
    IFileStorageService fileStorage,
    IDocumentProcessingQueue processingQueue)
    : IRequestHandler<UploadDocumentoCommand, Guid>
{
    public async Task<Guid> Handle(UploadDocumentoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = currentUser.EmpresaId!.Value; // garantido pelo middleware de autenticação/tenant
        var usuarioId = currentUser.UsuarioId!.Value;

        var caminhoArmazenamento = await fileStorage.SalvarAsync(
            request.ConteudoArquivo, request.NomeArquivoOriginal, empresaId, cancellationToken);

        var documento = Documento.Criar(
            empresaId,
            usuarioId,
            request.NomeArquivoOriginal,
            caminhoArmazenamento,
            request.TipoArquivo,
            request.TamanhoBytes);

        dbContext.Documentos.Add(documento);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Só enfileira após o SaveChangesAsync ter sucesso - garante que o
        // Worker, ao pegar o job, sempre encontrará o Documento no banco.
        await processingQueue.EnfileirarProcessamentoAsync(documento.Id, cancellationToken);

        return documento.Id;
    }
}