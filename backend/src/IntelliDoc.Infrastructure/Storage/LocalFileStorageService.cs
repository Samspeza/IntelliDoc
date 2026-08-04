using IntelliDoc.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace IntelliDoc.Infrastructure.Storage;

public sealed class LocalStorageSettings
{
    public const string SectionName = "LocalStorage";

    /// <summary>Diretório raiz (montado como volume Docker - ver docker-compose.yml, Etapa 14).</summary>
    public string DiretorioRaiz { get; init; } = "/data/storage";
}

/// <summary>
/// Implementação padrão de IFileStorageService (Application, Etapa 9.3):
/// grava os arquivos em um volume Docker local, organizados por empresa
/// (isolamento apenas organizacional - o isolamento de SEGURANÇA real é o
/// Global Query Filter no banco, RN02; nada impede fisicamente a leitura
/// cross-tenant no disco, por isso o caminho no disco nunca é exposto
/// diretamente ao frontend, sempre mediado por este serviço).
/// Decisão de troca futura para S3-compatible documentada em
/// docs/04-arquitetura.md (§3).
/// </summary>
public sealed class LocalFileStorageService(IOptions<LocalStorageSettings> settings) : IFileStorageService
{
    private readonly string _raiz = settings.Value.DiretorioRaiz;

    public async Task<string> SalvarAsync(Stream conteudo, string nomeArquivoOriginal, Guid empresaId, CancellationToken cancellationToken)
    {
        var extensao = Path.GetExtension(nomeArquivoOriginal);
        var nomeUnico = $"{Guid.NewGuid()}{extensao}";
        var diretorioEmpresa = Path.Combine(_raiz, empresaId.ToString());

        Directory.CreateDirectory(diretorioEmpresa);

        var caminhoCompleto = Path.Combine(diretorioEmpresa, nomeUnico);

        await using (var arquivoDestino = File.Create(caminhoCompleto))
        {
            await conteudo.CopyToAsync(arquivoDestino, cancellationToken);
        }

        // Caminho relativo persistido em Documento.CaminhoArmazenamento -
        // nunca o caminho absoluto do disco, para não vazar detalhes de
        // infraestrutura caso este valor seja logado ou exposto por engano.
        return Path.Combine(empresaId.ToString(), nomeUnico).Replace('\\', '/');
    }

    public Task<Stream> ObterAsync(string caminhoArmazenamento, CancellationToken cancellationToken)
    {
        var caminhoCompleto = Path.Combine(_raiz, caminhoArmazenamento);

        if (!File.Exists(caminhoCompleto))
        {
            throw new FileNotFoundException("Arquivo não encontrado no storage.", caminhoArmazenamento);
        }

        Stream stream = File.OpenRead(caminhoCompleto);
        return Task.FromResult(stream);
    }

    public Task ExcluirAsync(string caminhoArmazenamento, CancellationToken cancellationToken)
    {
        var caminhoCompleto = Path.Combine(_raiz, caminhoArmazenamento);

        if (File.Exists(caminhoCompleto))
        {
            File.Delete(caminhoCompleto);
        }

        return Task.CompletedTask;
    }
}