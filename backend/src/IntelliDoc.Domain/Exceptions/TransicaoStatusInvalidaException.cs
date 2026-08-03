using IntelliDoc.Domain.Enums;

namespace IntelliDoc.Domain.Exceptions;

/// <summary>
/// Lançada por Documento.TransicionarStatus quando a transição solicitada
/// não é permitida pela máquina de estados definida em
/// docs/07-diagrama-entidades-dominio.md (§3). Garante, por construção, que
/// RN19 (um documento só pode ser aprovado/rejeitado a partir de
/// AguardandoRevisao) nunca seja violada, mesmo por um bug em camada
/// superior.
/// </summary>
public sealed class TransicaoStatusInvalidaException : DomainException
{
    public StatusDocumento StatusAtual { get; }
    public StatusDocumento StatusSolicitado { get; }

    public TransicaoStatusInvalidaException(StatusDocumento statusAtual, StatusDocumento statusSolicitado)
        : base($"Não é possível transicionar o documento de '{statusAtual}' para '{statusSolicitado}'.")
    {
        StatusAtual = statusAtual;
        StatusSolicitado = statusSolicitado;
    }
}