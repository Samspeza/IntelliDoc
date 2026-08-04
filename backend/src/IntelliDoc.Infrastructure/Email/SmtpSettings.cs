namespace IntelliDoc.Infrastructure.Email;

public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; init; } = "smtp";
    public int Porta { get; init; } = 1025;
    public string RemetenteNome { get; init; } = "IntelliDoc";
    public string RemetenteEmail { get; init; } = "no-reply@intellidoc.local";
    public string? Usuario { get; init; }
    public string? Senha { get; init; }
    public bool UsarSsl { get; init; } = false;
}