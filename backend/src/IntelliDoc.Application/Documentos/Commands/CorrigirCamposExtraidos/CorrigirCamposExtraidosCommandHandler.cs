using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Application.Common.Interfaces;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliDoc.Application.Documentos.Commands.CorrigirCamposExtraidos;

/// <summary>
/// Executa o UC21. A regra de "só corrige se estiver AguardandoRevisao" e a
/// preservação do ValorExtraidoIa (RN20) vivem no Domain
/// (Documento.CorrigirCampo / CampoExtraido.Corrigir) - aqui apenas
/// validamos o papel (RN18) e orquestramos.
/// </summary>
public sealed class CorrigirCamposExtraidosCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<CorrigirCamposExtraidosCommand>
{
    public async Task Handle(CorrigirCamposExtraidosCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.TemPapel(PapelUsuario.Revisor) && !currentUser.TemPapel(PapelUsuario.Gestor))
        {
            throw new ForbiddenAccessException("Apenas revisores e gestores podem corrigir campos extraídos.");
        }

        var documento = await dbContext.Documentos
            .Include(d => d.Campos)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Documento), request.DocumentoId);

        foreach (var correcao in request.Correcoes)
        {
            documento.CorrigirCampo(correcao.NomeCampo, correcao.NovoValor);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}