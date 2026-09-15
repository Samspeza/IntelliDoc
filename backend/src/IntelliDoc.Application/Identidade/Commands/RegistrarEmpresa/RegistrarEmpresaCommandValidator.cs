using FluentValidation;

namespace IntelliDoc.Application.Identidade.Commands.RegistrarEmpresa;

/// <summary>
/// Valida a forma dos dados de cadastro, incluindo a política mínima de
/// senha exigida por RN09 (8+ caracteres, ao menos uma maiúscula e um
/// número). A unicidade do e-mail depende do banco e é verificada no
/// Handler, não aqui.
/// </summary>
public sealed class RegistrarEmpresaCommandValidator : AbstractValidator<RegistrarEmpresaCommand>
{
    public RegistrarEmpresaCommandValidator()
    {
        RuleFor(c => c.NomeEmpresa)
            .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
            .MaximumLength(200);

        RuleFor(c => c.CnpjOuIdentificador)
            .MaximumLength(20);

        RuleFor(c => c.NomeAdministrador)
            .NotEmpty().WithMessage("O nome do administrador é obrigatório.")
            .MaximumLength(150);

        RuleFor(c => c.Email)
            .NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(c => c.Senha)
            .NotEmpty()
            .MinimumLength(8).WithMessage("A senha deve ter ao menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
    }
}