using FluentValidation;

namespace IntelliDoc.Application.Documentos.Commands.UploadDocumento;

/// <summary>
/// Validação de FORMA do comando (RN11 - tipo e tamanho de arquivo).
/// Validações que dependem de estado do banco (ex.: se a empresa aceita
/// este tipo de documento, conforme ConfiguracaoEmpresa.TiposDocumentoAceitos)
/// ficam no Handler, não aqui - o Validator não tem acesso ao
/// IApplicationDbContext por design, para manter validações de forma e de
/// regra de negócio claramente separadas.
/// </summary>
public sealed class UploadDocumentoCommandValidator : AbstractValidator<UploadDocumentoCommand>
{
    private static readonly string[] TiposAceitos = ["pdf", "jpg", "png"];
    private const long TamanhoMaximoBytes = 10 * 1024 * 1024;

    public UploadDocumentoCommandValidator()
    {
        RuleFor(c => c.NomeArquivoOriginal)
            .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
            .MaximumLength(300);

        RuleFor(c => c.TipoArquivo)
            .NotEmpty()
            .Must(tipo => TiposAceitos.Contains(tipo.ToLowerInvariant()))
            .WithMessage($"Tipo de arquivo inválido. Tipos aceitos: {string.Join(", ", TiposAceitos)}.");

        RuleFor(c => c.TamanhoBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(TamanhoMaximoBytes)
            .WithMessage("O arquivo deve ter até 10MB.");

        RuleFor(c => c.ConteudoArquivo)
            .NotNull();
    }
}