namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai onde os arquivos originais dos documentos são armazenados.
/// Implementada por Infrastructure.Storage.LocalFileStorageService (volume
/// Docker), com troca futura para provedor S3-compatible documentada em
/// docs/04-arquitetura.md (§3) sem impacto nesta interface.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Salva o arquivo e retorna o caminho/chave de armazenamento (persistido em Documento.CaminhoArmazenamento).</summary>
    Task<string> SalvarAsync(Stream conteudo, string nomeArquivoOriginal, Guid empresaId, CancellationToken cancellationToken);

    Task<Stream> ObterAsync(string caminhoArmazenamento, CancellationToken cancellationToken);

    Task ExcluirAsync(string caminhoArmazenamento, CancellationToken cancellationToken);
}