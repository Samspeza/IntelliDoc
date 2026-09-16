using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Documentos.Commands.RejeitarDocumento;

/// <summary>
/// Executa o UC23. Simétrico ao AprovarDocumentoCommandHandler, exceto por
/// não precisar consultar a ConfiguracaoEmpresa: a segregação (RN24)
/// restringe apenas a APROVAÇÃO - rejeitar o próprio documento não cria
/// conflito de interesse.
/// </summary>
public sealed class RejeitarDocumentoCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser,
    ICacheService cacheService) : IRequestHandler<RejeitarDocumentoCommand>
{
    public async Task Handle(RejeitarDocumentoCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.TemPapel(PapelUsuario.Revisor) && !currentUser.TemPapel(PapelUsuario.Gestor))
        {
            throw new ForbiddenAccessException("Apenas revisores e gestores podem rejeitar documentos.");
        }

        var usuarioId = currentUser.UsuarioId!.Value;

        var documento = await dbContext.Documentos
            .FirstOrDefaultAsync(d => d.Id == request.DocumentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Documento), request.DocumentoId);

        documento.Rejeitar(usuarioId, request.Motivo);

        dbContext.RegistrosAuditoria.Add(RegistroAuditoria.Criar(
            empresaId: documento.EmpresaId,
            usuarioId: usuarioId,
            acao: TipoAcaoAuditoria.RejeitarDocumento,
            entidadeAfetada: nameof(Documento),
            entidadeAfetadaId: documento.Id,
            dadosAntesJson: $$"""{"status":"{{StatusDocumento.AguardandoRevisao}}"}""",
            dadosDepoisJson: $$"""{"status":"{{StatusDocumento.Rejeitado}}","motivo":"{{request.Motivo}}"}""",
            enderecoIp: currentUser.EnderecoIp));

        await dbContext.SaveChangesAsync(cancellationToken);

        await cacheService.RemoverPorPrefixoAsync($"dashboard:{documento.EmpresaId}:", cancellationToken);
    }
}