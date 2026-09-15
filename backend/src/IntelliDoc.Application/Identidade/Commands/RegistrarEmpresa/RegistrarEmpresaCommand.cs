using IntelliDoc.Application.Common.Models;
using MediatR;

namespace IntelliDoc.Application.Identidade.Commands.RegistrarEmpresa;

/// <summary>
/// UC01: onboarding self-service. Cria a Empresa (tenant) e seu primeiro
/// usuário, que recebe o papel AdminEmpresa. Retorna já os tokens
/// autenticados, evitando exigir um login imediatamente após o cadastro.
/// </summary>
public sealed record RegistrarEmpresaCommand(
    string NomeEmpresa,
    string? CnpjOuIdentificador,
    string NomeAdministrador,
    string Email,
    string Senha) : IRequest<RespostaAutenticacao>;