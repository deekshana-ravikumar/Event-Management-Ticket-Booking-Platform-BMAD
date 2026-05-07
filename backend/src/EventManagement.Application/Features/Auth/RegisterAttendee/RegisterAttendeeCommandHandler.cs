using EventManagement.Application.Common.Interfaces;
using EventManagement.Application.Common.Settings;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Enums;
using EventManagement.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventManagement.Application.Features.Auth.RegisterAttendee;

public sealed class RegisterAttendeeCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    IConsentLedgerService consentLedger,
    IOptions<FrontendSettings> frontendSettings)
    : IRequestHandler<RegisterAttendeeCommand, RegisterAttendeeResult>
{
    private readonly FrontendSettings _frontend = frontendSettings.Value;

    public async Task<RegisterAttendeeResult> Handle(
        RegisterAttendeeCommand request,
        CancellationToken cancellationToken)
    {
        var normalised = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == normalised, cancellationToken))
            throw new DuplicateEmailException();

        var user = new User
        {
            Email = normalised,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            City = request.City.Trim(),
            Role = UserRole.Attendee,
            Status = UserStatus.PendingVerification
        };

        var rawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var verificationToken = new EmailVerificationToken
        {
            Token = rawToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        user.EmailVerificationTokens.Add(verificationToken);
        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);

        await consentLedger.RecordConsentAsync(
            user.Id,
            ConsentType.Registration,
            request.TncVersion,
            request.IpAddress,
            request.UserAgent,
            cancellationToken);

        var verificationUrl = $"{_frontend.BaseUrl}/verify-email?token={Uri.EscapeDataString(rawToken)}";
        await emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationUrl, cancellationToken);

        return new RegisterAttendeeResult(user.Id, user.Email, user.Status.ToString());
    }
}
