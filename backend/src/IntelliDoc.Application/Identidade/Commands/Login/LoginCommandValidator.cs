using FluentValidation;

namespace IntelliDoc.Application.Identidade.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();

        // Sem validação de política de senha aqui de propósito: no LOGIN,
        // rejeitar por "senha fraca" antes de verificar as credenciais
        // revelaria informação sobre o formato esperado e criaria respostas
        // diferentes para senha malformada vs. senha errada.
        RuleFor(c => c.Senha).NotEmpty();
    }
}