using MediatR;

namespace EventManagement.Application.Features.Auth.Login;

public record LoginCommand : IRequest<LoginResult>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public record LoginResult(
    string AccessToken,
    int ExpiresIn,
    long UserId,
    string Email,
    string Role,
    string? OrgStatus,

    /// <summary>Raw refresh token — controller sets as httpOnly cookie. Never expose in response body.</summary>
    string RawRefreshToken);
