namespace IntelliDoc.Domain.Enums;

/// <summary>
/// Status possíveis de um Documento, conforme a máquina de estados definida
/// em docs/07-diagrama-entidades-dominio.md (§3):
///
///   Enviado -> Processando -> AguardandoRevisao -> Aprovado
///                           -> AguardandoRevisao -> Rejeitado -> Enviado
///   Processando -> FalhaProcessamento -> Enviado (retry, RN14)
///   {Enviado, Aprovado, Rejeitado} -> Arquivado (RN17)
///
/// A validação de quais transições são permitidas vive em
/// Documento.TransicionarStatus (Domain), não aqui - este enum é apenas o
/// conjunto de estados possíveis.
/// </summary>
public enum StatusDocumento
{
    Enviado = 0,
    Processando = 1,
    AguardandoRevisao = 2,
    Aprovado = 3,
    Rejeitado = 4,
    FalhaProcessamento = 5,
    Arquivado = 6
}