using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Documentos.Commands.AprovarDocumento;

/// <summary>
/// Executa o UC22. Divisão de responsabilidades (decisão da Etapa 7 §5):
///   - Handler (aqui): valida o PAPEL do usuário (RN18) e BUSCA a
///     configuração da empresa para saber se a segregação está ativa (RN24).
///   - Domain (Documento.Aprovar): valida a transição de estado (RN19),
///     aplica a regra de segregação com o dado recebido, registra o
///     histórico (RN23) e dispara DocumentoAprovadoEvent.
///
/// A auditoria (RN32) e a invalidação do cache do dashboard acontecem aqui,
/// pois são preocupações de aplicação/infraestrutura, não de domínio.
/// </summary>
public sealed class AprovarDocumentoCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser,
    ICacheService cacheService) : IRequestHandler<AprovarDocumentoCommand>
{
    public async Task Handle(AprovarDocumentoCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.TemPapel(PapelUsuario.Revisor) && !currentUser.TemPapel(PapelUsuario.Gestor))
        {
            throw new ForbiddenAccessException("Apenas revisores e gestores podem aprovar documentos.");
        }

        var usuarioId = currentUser.UsuarioId!.Value;

        var documento = await dbContext.Documentos
            .Include(d => d.Campos)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Documento), request.DocumentoId);

        var empresa = await dbContext.Empresas
            .Include(e => e.Configuracao)
            .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Empresa), documento.EmpresaId);

        documento.Aprovar(usuarioId, empresa.Configuracao.SegregacaoRevisorAtiva);

        dbContext.RegistrosAuditoria.Add(RegistroAuditoria.Criar(
            empresaId: documento.EmpresaId,
            usuarioId: usuarioId,
            acao: TipoAcaoAuditoria.AprovarDocumento,
            entidadeAfetada: nameof(Documento),
            entidadeAfetadaId: documento.Id,
            dadosAntesJson: $$"""{"status":"{{StatusDocumento.AguardandoRevisao}}"}""",
            dadosDepoisJson: $$"""{"status":"{{StatusDocumento.Aprovado}}"}""",
            enderecoIp: currentUser.EnderecoIp));

        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidação por evento dos indicadores do dashboard (Etapa 4 §8) -
        // uma aprovação muda taxa de aprovação e contagem de pendências.
        await cacheService.RemoverPorPrefixoAsync($"dashboard:{documento.EmpresaId}:", cancellationToken);
    }
}