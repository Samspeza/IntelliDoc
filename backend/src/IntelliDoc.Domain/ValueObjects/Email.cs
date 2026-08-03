using System.Text.RegularExpressions;
using IntelliDoc.Domain.Exceptions;

namespace IntelliDoc.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um endereço de e-mail válido. Uma vez
/// instanciado via Email.Criar, garante-se que o valor é sintaticamente
/// válido - elimina a necessidade de validar o formato em cada camada que
/// manipula um e-mail (Application, Infrastructure).
/// Implementado como 'record' para obter igualdade estrutural gratuita
/// (dois Email com o mesmo Valor são iguais), coerente com a semântica de
/// Value Object.
/// </summary>
public sealed partial record Email
{
    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }

    public static Email Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new RegraDeNegocioException("EMAIL_VAZIO", "O e-mail não pode ser vazio.");
        }

        var normalizado = valor.Trim().ToLowerInvariant();

        if (normalizado.Length > 256 || !FormatoEmailRegex().IsMatch(normalizado))
        {
            throw new RegraDeNegocioException("EMAIL_INVALIDO", $"O e-mail '{valor}' não é válido.");
        }

        return new Email(normalizado);
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex FormatoEmailRegex();
}