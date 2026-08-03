namespace IntelliDoc.Domain.Exceptions;

/// <summary>
/// Lançada quando uma regra de negócio explícita (RN) é violada dentro de
/// uma entidade de domínio - por exemplo, motivo de rejeição abaixo do
/// mínimo de caracteres (RN21) ou tentativa de autoaprovação com segregação
/// ativa (RN24). Para violações especificamente de transição de estado do
/// Documento, usar TransicaoStatusInvalidaException, que carrega mais
/// contexto estruturado.
/// </summary>
public sealed class RegraDeNegocioException : DomainException
{
    public string CodigoRegra { get; }

    public RegraDeNegocioException(string codigoRegra, string message) : base(message)
    {
        CodigoRegra = codigoRegra;
    }
}