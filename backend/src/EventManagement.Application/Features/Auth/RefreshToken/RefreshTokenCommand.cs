using MediatR;

namespace EventManagement.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RawRefreshToken) : IRequest<RefreshTokenResult>;

public record RefreshTokenResult(string AccessToken, int ExpiresIn, string NewRawRefreshToken);
