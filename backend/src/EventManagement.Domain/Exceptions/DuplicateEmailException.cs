namespace EventManagement.Domain.Exceptions;

public sealed class DuplicateEmailException : DomainException
{
    public DuplicateEmailException() : base("Email already registered.") { }
}
