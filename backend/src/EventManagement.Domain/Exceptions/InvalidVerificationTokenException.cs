namespace EventManagement.Domain.Exceptions;

public sealed class InvalidVerificationTokenException : DomainException
{
    public InvalidVerificationTokenException() : base("This verification link has already been used.") { }
}
