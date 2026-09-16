namespace IntelliDoc.Application.Notificacoes.Dtos;

public sealed record NotificacaoDto(
    Guid Id,
    string Titulo,
    string Mensagem,
    bool Lida,
    Guid? DocumentoRelacionadoId,
    DateTime CriadoEm);