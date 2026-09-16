namespace IntelliDoc.Application.Auditoria.Dtos;

public sealed record RegistroAuditoriaDto(
    Guid Id,
    Guid? UsuarioId,
    string Acao,
    string EntidadeAfetada,
    Guid? EntidadeAfetadaId,
    string? DadosAntes,
    string? DadosDepois,
    string EnderecoIp,
    DateTime CriadoEm);