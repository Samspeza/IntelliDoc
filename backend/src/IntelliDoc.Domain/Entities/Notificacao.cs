using IntelliDoc.Domain.Common;
using IntelliDoc.Domain.Enums;
using IntelliDoc.Domain.Exceptions;

namespace IntelliDoc.Domain.Entities;

/// <summary>
/// Aggregate Root simples (sem entidades filhas). Representa uma
/// notificação in-app, endereçada a um usuário específico (RF22/RN30) ou a
/// todos os usuários com um determinado papel na empresa, de forma agregada
/// (RF21/RN29) - por isso UsuarioDestinoId e PapelDestino são mutuamente
/// alternativos (validado em Criar).
/// </summary>
public sealed class Notificacao : AggregateRoot, IAuditavel
{
    public Guid EmpresaId { get; private set; }
    public Guid? UsuarioDestinoId { get; private set; }
    public PapelUsuario? PapelDestino { get; private set; }
    public string Titulo { get; private set; }
    public string Mensagem { get; private set; }
    public bool Lida { get; private set; }
    public Guid? DocumentoRelacionadoId { get; private set; }

    private Notificacao()
    {
        Titulo = string.Empty;
        Mensagem = string.Empty;
    }

    private Notificacao(
        Guid empresaId,
        Guid? usuarioDestinoId,
        PapelUsuario? papelDestino,
        string titulo,
        string mensagem,
        Guid? documentoRelacionadoId)
    {
        EmpresaId = empresaId;
        UsuarioDestinoId = usuarioDestinoId;
        PapelDestino = papelDestino;
        Titulo = titulo;
        Mensagem = mensagem;
        DocumentoRelacionadoId = documentoRelacionadoId;
        Lida = false;
    }

    /// <summary>RN30: notificação individual, ex.: "seu documento foi aprovado" (UC30).</summary>
    public static Notificacao CriarParaUsuario(
        Guid empresaId, Guid usuarioDestinoId, string titulo, string mensagem, Guid? documentoRelacionadoId = null)
    {
        ValidarTextos(titulo, mensagem);
        return new Notificacao(empresaId, usuarioDestinoId, papelDestino: null, titulo.Trim(), mensagem.Trim(), documentoRelacionadoId);
    }

    /// <summary>RN29: notificação agregada por papel, ex.: "há documentos pendentes de revisão" (UC29).</summary>
    public static Notificacao CriarParaPapel(
        Guid empresaId, PapelUsuario papelDestino, string titulo, string mensagem, Guid? documentoRelacionadoId = null)
    {
        ValidarTextos(titulo, mensagem);
        return new Notificacao(empresaId, usuarioDestinoId: null, papelDestino, titulo.Trim(), mensagem.Trim(), documentoRelacionadoId);
    }

    private static void ValidarTextos(string titulo, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new RegraDeNegocioException("NOTIFICACAO_TITULO_OBRIGATORIO", "O título da notificação é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(mensagem))
        {
            throw new RegraDeNegocioException("NOTIFICACAO_MENSAGEM_OBRIGATORIA", "A mensagem da notificação é obrigatória.");
        }
    }

    public void MarcarComoLida() => Lida = true;
}