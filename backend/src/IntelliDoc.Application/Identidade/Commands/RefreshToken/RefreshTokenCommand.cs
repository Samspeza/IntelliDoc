using IntelliDoc.Application.Common.Models;
using MediatR;

namespace IntelliDoc.Application.Identidade.Commands.RefreshToken;

/// <summary>UC03: renova a sessão a partir de um refresh token válido.</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<RespostaAutenticacao>;