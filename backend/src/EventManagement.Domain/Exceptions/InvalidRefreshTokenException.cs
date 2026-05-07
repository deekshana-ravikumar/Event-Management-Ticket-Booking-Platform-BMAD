namespace EventManagement.Domain.Exceptions;

public sealed class InvalidRefreshTokenException : DomainException
{
    public InvalidRefreshTokenException() : base("Refresh token is invalid or has expired.") { }
}
