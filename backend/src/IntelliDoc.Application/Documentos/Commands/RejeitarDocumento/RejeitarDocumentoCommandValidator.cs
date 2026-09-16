using FluentValidation;

namespace IntelliDoc.Application.Documentos.Commands.RejeitarDocumento;

/// <summary>
/// RN21: motivo com mínimo de 10 caracteres. A mesma regra também existe no
/// Domain (Documento.Rejeitar) - duplicação DELIBERADA: aqui ela produz uma
/// mensagem de validação amigável por campo (HTTP 400 estruturado), e lá
/// ela garante que a invariante nunca seja violada mesmo se o Command for
/// chamado por outro caminho (ex.: um job, um teste).
/// </summary>
public sealed class RejeitarDocumentoCommandValidator : AbstractValidator<RejeitarDocumentoCommand>
{
    public RejeitarDocumentoCommandValidator()
    {
        RuleFor(c => c.DocumentoId).NotEmpty();

        RuleFor(c => c.Motivo)
            .NotEmpty().WithMessage("O motivo da rejeição é obrigatório.")
            .MinimumLength(10).WithMessage("O motivo da rejeição deve ter ao menos 10 caracteres.")
            .MaximumLength(500);
    }
}