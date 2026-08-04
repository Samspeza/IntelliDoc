using IntelliDoc.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IntelliDoc.Infrastructure.Email;

/// <summary>
/// Implementação de IEmailService (Application, Etapa 9.3) via SMTP
/// (MailKit). Em ambiente de desenvolvimento (docker-compose), aponta para
/// um servidor SMTP de teste (ex.: MailHog/Papercut) que captura os
/// e-mails sem enviá-los de fato - configurável via appsettings.
/// Conforme RN31: falha de envio é apenas logada, nunca lançada como
/// exceção - o chamador (ex.: AprovarDocumentoCommandHandler) não deve
/// falhar a operação principal (aprovar o documento) só porque o e-mail de
/// notificação não pôde ser enviado.
/// </summary>
public sealed class SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = settings.Value;

    public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml, CancellationToken cancellationToken)
    {
        try
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(_settings.RemetenteNome, _settings.RemetenteEmail));
            mensagem.To.Add(MailboxAddress.Parse(destinatario));
            mensagem.Subject = assunto;
            mensagem.Body = new BodyBuilder { HtmlBody = corpoHtml }.ToMessageBody();

            using var cliente = new SmtpClient();

            var opcaoSsl = _settings.UsarSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await cliente.ConnectAsync(_settings.Host, _settings.Porta, opcaoSsl, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Usuario))
            {
                await cliente.AuthenticateAsync(_settings.Usuario, _settings.Senha, cancellationToken);
            }

            await cliente.SendAsync(mensagem, cancellationToken);
            await cliente.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            // RN31: falha de e-mail é apenas logada, não interrompe o fluxo de negócio.
            logger.LogWarning(ex, "Falha ao enviar e-mail para {Destinatario} com assunto '{Assunto}'", destinatario, assunto);
        }
    }
}