using MediatR;

namespace IntelliDoc.Application.Identidade.Commands.Logout;

/// <summary>UC06: encerra a sessão, revogando o refresh token corrente (RN10).</summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest;