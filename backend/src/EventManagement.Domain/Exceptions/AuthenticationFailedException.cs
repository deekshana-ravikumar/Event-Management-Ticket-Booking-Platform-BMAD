namespace EventManagement.Domain.Exceptions;

public sealed class AuthenticationFailedException : DomainException
{
    public AuthenticationFailedException(string message) : base(message) { }
}
