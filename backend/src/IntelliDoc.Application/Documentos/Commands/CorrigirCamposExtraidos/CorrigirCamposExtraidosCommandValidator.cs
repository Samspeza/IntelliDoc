using FluentValidation;

namespace IntelliDoc.Application.Documentos.Commands.CorrigirCamposExtraidos;

public sealed class CorrigirCamposExtraidosCommandValidator : AbstractValidator<CorrigirCamposExtraidosCommand>
{
    public CorrigirCamposExtraidosCommandValidator()
    {
        RuleFor(c => c.DocumentoId).NotEmpty();

        RuleFor(c => c.Correcoes)
            .NotEmpty().WithMessage("Informe ao menos um campo para corrigir.");

        RuleForEach(c => c.Correcoes).ChildRules(correcao =>
        {
            correcao.RuleFor(x => x.NomeCampo).NotEmpty().MaximumLength(100);
            correcao.RuleFor(x => x.NovoValor).NotNull().MaximumLength(500);
        });
    }
}