using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Enums;
using IntelliDoc.Domain.Events;
using IntelliDoc.Domain.Exceptions;
using IntelliDoc.Domain.ValueObjects;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Aggregate Root central do sistema (docs/07-diagrama-entidades-dominio.md,
/// §1). Agrega CampoExtraido e HistoricoStatusDocumento - nenhuma das duas
/// entidades filhas é alterada fora deste agregado.
///
/// A máquina de estados (§3 do mesmo documento) é garantida pelo método
/// privado TransicionarStatus: é o único lugar do sistema que altera
/// Status, e valida a transição antes de aplicá-la.
/// </summary>
public sealed class Documento : AggregateRoot, IAuditavel
{
    private const int MaxTentativasProcessamento = 3; // RN14
    private const int TamanhoMaximoBytes = 10 * 1024 * 1024; // RN11 - 10MB
    private static readonly string[] TiposArquivoAceitos = ["pdf", "jpg", "png"]; // RN11

    public Guid EmpresaId { get; private set; }
    public Guid EnviadoPorUsuarioId { get; private set; }
    public string NomeArquivoOriginal { get; private set; }
    public string CaminhoArmazenamento { get; private set; }
    public string TipoArquivo { get; private set; }
    public long TamanhoBytes { get; private set; }
    public TipoDocumento TipoDocumento { get; private set; }
    public StatusDocumento Status { get; private set; }
    public bool PrioridadeRevisao { get; private set; }
    public ConfidenceScore? ScoreMedio { get; private set; }
    public int TentativasProcessamento { get; private set; }
    public string? MotivoRejeicao { get; private set; }
    public bool Arquivado { get; private set; }

    private readonly List<CampoExtraido> _campos = [];
    public IReadOnlyCollection<CampoExtraido> Campos => _campos.AsReadOnly();

    private readonly List<HistoricoStatusDocumento> _historico = [];
    public IReadOnlyCollection<HistoricoStatusDocumento> Historico => _historico.AsReadOnly();

    private Documento()
    {
        NomeArquivoOriginal = string.Empty;
        CaminhoArmazenamento = string.Empty;
        TipoArquivo = string.Empty;
    }

    private Documento(
        Guid empresaId,
        Guid enviadoPorUsuarioId,
        string nomeArquivoOriginal,
        string caminhoArmazenamento,
        string tipoArquivo,
        long tamanhoBytes)
    {
        EmpresaId = empresaId;
        EnviadoPorUsuarioId = enviadoPorUsuarioId;
        NomeArquivoOriginal = nomeArquivoOriginal;
        CaminhoArmazenamento = caminhoArmazenamento;
        TipoArquivo = tipoArquivo;
        TamanhoBytes = tamanhoBytes;
        TipoDocumento = TipoDocumento.NaoClassificado;
        Status = StatusDocumento.Enviado;
        TentativasProcessamento = 0;
    }

    /// <summary>
    /// RF06/RN11/RN12/RN13: cria o documento no upload (UC13). Já nasce
    /// logicamente pronto para ser enfileirado - o enfileiramento físico na
    /// fila (Hangfire) é responsabilidade do Command Handler via
    /// IDocumentProcessingQueue, não deste método.
    /// </summary>
    public static Documento Criar(
        Guid empresaId,
        Guid enviadoPorUsuarioId,
        string nomeArquivoOriginal,
        string caminhoArmazenamento,
        string tipoArquivo,
        long tamanhoBytes)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivoOriginal))
        {
            throw new RegraDeNegocioException("DOCUMENTO_NOME_OBRIGATORIO", "O nome do arquivo é obrigatório.");
        }

        var tipoNormalizado = tipoArquivo.Trim().ToLowerInvariant();
        if (!TiposArquivoAceitos.Contains(tipoNormalizado))
        {
            throw new RegraDeNegocioException(
                "DOCUMENTO_TIPO_ARQUIVO_INVALIDO",
                $"Tipo de arquivo '{tipoArquivo}' não é aceito. Tipos aceitos: {string.Join(", ", TiposArquivoAceitos)}.");
        }

        if (tamanhoBytes <= 0 || tamanhoBytes > TamanhoMaximoBytes)
        {
            throw new RegraDeNegocioException(
                "DOCUMENTO_TAMANHO_INVALIDO",
                $"O arquivo deve ter até {TamanhoMaximoBytes / 1024 / 1024}MB.");
        }

        return new Documento(
            empresaId,
            enviadoPorUsuarioId,
            nomeArquivoOriginal.Trim(),
            caminhoArmazenamento,
            tipoNormalizado,
            tamanhoBytes);
    }

    /// <summary>UC14/RN13: chamado pelo Worker ao pegar o job da fila.</summary>
    public void IniciarProcessamento() => TransicionarStatus(StatusDocumento.Processando, usuarioId: null, motivo: null);

    /// <summary>
    /// RF08/RF09/RF10/RN15/RN16: chamado pelo Worker após OCR + extração de
    /// campos ter sucesso. Calcula o score médio, aplica a classificação de
    /// tipo, e marca PrioridadeRevisao se o score médio ficar abaixo do
    /// limiar configurado pela empresa (o limiar é passado como parâmetro -
    /// o Domínio não conhece ConfiguracaoEmpresa diretamente, mantendo o
    /// agregado desacoplado, conforme decisão registrada na Etapa 7 §5).
    /// </summary>
    public void RegistrarResultadoExtracao(
        List<CampoExtraido> camposExtraidos,
        TipoDocumento tipoClassificado,
        decimal limiarConfiancaEmpresa)
    {
        if (camposExtraidos.Count == 0)
        {
            throw new RegraDeNegocioException(
                "DOCUMENTO_SEM_CAMPOS_EXTRAIDOS",
                "O resultado da extração deve conter ao menos um campo.");
        }

        _campos.Clear();
        _campos.AddRange(camposExtraidos);

        TipoDocumento = tipoClassificado;
        ScoreMedio = ConfidenceScore.CalcularMedia(camposExtraidos.Select(c => c.Confidence));
        PrioridadeRevisao = ScoreMedio.EstaAbaixoDoLimiar(limiarConfiancaEmpresa);

        TransicionarStatus(StatusDocumento.AguardandoRevisao, usuarioId: null, motivo: null);
    }

    /// <summary>RN14: falha no OCR/IA - incrementa tentativa e move para FalhaProcessamento.</summary>
    public void MarcarFalhaProcessamento()
    {
        TentativasProcessamento++;
        TransicionarStatus(StatusDocumento.FalhaProcessamento, usuarioId: null, motivo: null);
    }

    /// <summary>RN14: reprocessamento automático só é permitido até 3 tentativas.</summary>
    public bool PodeReprocessar() => TentativasProcessamento < MaxTentativasProcessamento;

    /// <summary>
    /// UC17/RN14/RN22: reenvia um documento com FalhaProcessamento ou
    /// Rejeitado de volta para a fila (Enviado). Diferente do retry
    /// automático (RN14), o reenvio manual após rejeição não conta para o
    /// limite de tentativas de OCR - reseta o contador, pois é uma nova
    /// tentativa deliberada do usuário.
    /// </summary>
    public void Reenviar(Guid usuarioId)
    {
        if (Status == StatusDocumento.FalhaProcessamento && !PodeReprocessar())
        {
            throw new RegraDeNegocioException(
                "DOCUMENTO_LIMITE_TENTATIVAS_EXCEDIDO",
                $"O documento já atingiu o limite de {MaxTentativasProcessamento} tentativas de processamento.");
        }

        if (Status == StatusDocumento.Rejeitado)
        {
            TentativasProcessamento = 0;
        }

        TransicionarStatus(StatusDocumento.Enviado, usuarioId, motivo: null);
    }

    /// <summary>
    /// RF13/RN20: revisor corrige um campo extraído. O valor original da IA
    /// (ValorExtraidoIa) é preservado dentro de CampoExtraido.Corrigir -
    /// nunca é sobrescrito, apenas ValorFinal muda.
    /// </summary>
    public void CorrigirCampo(string nomeCampo, string novoValor)
    {
        if (Status != StatusDocumento.AguardandoRevisao)
        {
            throw new RegraDeNegocioException(
                "DOCUMENTO_FORA_DE_REVISAO",
                "Só é possível corrigir campos de um documento que está aguardando revisão.");
        }

        var campo = _campos.FirstOrDefault(c => c.NomeCampo == nomeCampo)
            ?? throw new RegraDeNegocioException("CAMPO_NAO_ENCONTRADO", $"Campo '{nomeCampo}' não encontrado neste documento.");

        campo.Corrigir(novoValor);
    }

    /// <summary>
    /// UC22/RN18/RN19/RN24: aprova o documento. `segregacaoAtiva` é passado
    /// pelo Command Handler (consultando ConfiguracaoEmpresa) - se ativa e o
    /// aprovador é quem enviou o documento, a operação é bloqueada (RN24).
    /// A checagem de PAPEL (só Revisor/Gestor podem aprovar) é feita na
    /// Application (AprovarDocumentoCommandHandler), não aqui: o Domínio não
    /// tem acesso aos papéis do usuário autenticado, apenas ao seu Id.
    /// </summary>
    public void Aprovar(Guid aprovadoPorUsuarioId, bool segregacaoAtiva)
    {
        if (segregacaoAtiva && aprovadoPorUsuarioId == EnviadoPorUsuarioId)
        {
            throw new RegraDeNegocioException(
                "AUTOAPROVACAO_BLOQUEADA",
                "Segregação de função ativa: você não pode aprovar um documento que você mesmo enviou.");
        }

        TransicionarStatus(StatusDocumento.Aprovado, aprovadoPorUsuarioId, motivo: null);

        AdicionarEvento(new DocumentoAprovadoEvent(Id, EmpresaId, EnviadoPorUsuarioId, aprovadoPorUsuarioId, NomeArquivoOriginal));
    }

    /// <summary>UC23/RN18/RN19/RN21: rejeita o documento com motivo obrigatório (mín. 10 caracteres).</summary>
    public void Rejeitar(Guid rejeitadoPorUsuarioId, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 10)
        {
            throw new RegraDeNegocioException(
                "MOTIVO_REJEICAO_INVALIDO",
                "O motivo da rejeição deve ter ao menos 10 caracteres.");
        }

        MotivoRejeicao = motivo.Trim();
        TransicionarStatus(StatusDocumento.Rejeitado, rejeitadoPorUsuarioId, motivo.Trim());

        AdicionarEvento(new DocumentoRejeitadoEvent(Id, EmpresaId, EnviadoPorUsuarioId, rejeitadoPorUsuarioId, NomeArquivoOriginal, motivo.Trim()));
    }

    /// <summary>RN17: soft delete - documento sai das listagens padrão, mas é preservado.</summary>
    public void Arquivar()
    {
        if (Status is StatusDocumento.Processando or StatusDocumento.AguardandoRevisao)
        {
            throw new RegraDeNegocioException(
                "DOCUMENTO_NAO_PODE_SER_ARQUIVADO",
                "Não é possível arquivar um documento que está em processamento ou aguardando revisão.");
        }

        Arquivado = true;
        TransicionarStatus(StatusDocumento.Arquivado, usuarioId: null, motivo: null);
    }

    /// <summary>
    /// Único ponto do sistema que altera Status. Valida a transição contra a
    /// máquina de estados definida em docs/07-diagrama-entidades-dominio.md
    /// (§3) e, se válida, aplica e registra em HistoricoStatusDocumento
    /// (RN23 - auditoria imutável de toda mudança de status).
    /// </summary>
    private void TransicionarStatus(StatusDocumento novoStatus, Guid? usuarioId, string? motivo)
    {
        if (!TransicaoPermitida(Status, novoStatus))
        {
            throw new TransicaoStatusInvalidaException(Status, novoStatus);
        }

        var statusAnterior = Status;
        Status = novoStatus;

        _historico.Add(HistoricoStatusDocumento.Criar(Id, statusAnterior, novoStatus, usuarioId, motivo));
    }

    private static bool TransicaoPermitida(StatusDocumento atual, StatusDocumento novo) => (atual, novo) switch
    {
        (StatusDocumento.Enviado, StatusDocumento.Processando) => true,
        (StatusDocumento.Processando, StatusDocumento.AguardandoRevisao) => true,
        (StatusDocumento.Processando, StatusDocumento.FalhaProcessamento) => true,
        (StatusDocumento.FalhaProcessamento, StatusDocumento.Enviado) => true,
        (StatusDocumento.AguardandoRevisao, StatusDocumento.Aprovado) => true,
        (StatusDocumento.AguardandoRevisao, StatusDocumento.Rejeitado) => true,
        (StatusDocumento.Rejeitado, StatusDocumento.Enviado) => true,
        (StatusDocumento.Enviado, StatusDocumento.Arquivado) => true,
        (StatusDocumento.Aprovado, StatusDocumento.Arquivado) => true,
        (StatusDocumento.Rejeitado, StatusDocumento.Arquivado) => true,
        _ => false
    };
}