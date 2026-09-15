using IntelliDoc.Application.Common.Models;
using MediatR;

namespace IntelliDoc.Application.Identidade.Commands.Login;

/// <summary>UC02: autenticação por e-mail e senha.</summary>
public sealed record LoginCommand(string Email, string Senha) : IRequest<RespostaAutenticacao>;