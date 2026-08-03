namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai o envio de e-mails transacionais (RF03, RF21, RF22).
/// Implementada por Infrastructure.Email.SmtpEmailService. Falhas de envio
/// são apenas logadas (RN31) - o método não lança exceção para o chamador
/// não precisar tratar falha de e-mail como erro de negócio.
/// </summary>
public interface IEmailService
{
    Task EnviarAsync(string destinatario, string assunto, string corpoHtml, CancellationToken cancellationToken);
}