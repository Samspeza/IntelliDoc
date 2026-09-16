using MediatR;

namespace IntelliDoc.Application.Documentos.Commands.CorrigirCamposExtraidos;

/// <summary>
/// UC21: revisor corrige um ou mais campos extraídos. Aceita uma LISTA de
/// correções em vez de um campo por requisição - o revisor tipicamente
/// ajusta vários campos de uma vez na tela de revisão, e um Command por
/// campo geraria N requisições e N registros de auditoria para uma única
/// ação do usuário.
/// </summary>
public sealed record CorrigirCamposExtraidosCommand(
    Guid DocumentoId,
    IReadOnlyCollection<CorrecaoCampo> Correcoes) : IRequest;

public sealed record CorrecaoCampo(string NomeCampo, string NovoValor);