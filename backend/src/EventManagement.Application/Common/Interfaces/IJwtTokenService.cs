namespace EventManagement.Application.Common.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Generates a signed JWT access token with 30-minute expiry.</summary>
    string GenerateAccessToken(long userId, string email, string role);

    /// <summary>Generates a cryptographically random raw refresh token (base64). Store its hash, not this value.</summary>
    string GenerateRefreshTokenRaw();

    /// <summary>Returns the SHA-256 hex digest of the raw token. This is what is persisted in DB.</summary>
    string HashToken(string rawToken);
}
