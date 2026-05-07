namespace EventManagement.Domain.Exceptions;

public sealed class VerificationTokenExpiredException : DomainException
{
    public VerificationTokenExpiredException() : base("Verification link has expired. Please request a new one.") { }
}
