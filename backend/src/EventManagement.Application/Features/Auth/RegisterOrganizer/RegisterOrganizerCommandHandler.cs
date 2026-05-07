using EventManagement.Application.Common.Interfaces;
using EventManagement.Application.Common.Settings;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Enums;
using EventManagement.Domain.Exceptions;
using EventManagement.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventManagement.Application.Features.Auth.RegisterOrganizer;

public sealed class RegisterOrganizerCommandHandler(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    IConsentLedgerService consentLedger,
    IOptions<FrontendSettings> frontendSettings)
    : IRequestHandler<RegisterOrganizerCommand, RegisterOrganizerResult>
{
    private readonly FrontendSettings _frontend = frontendSettings.Value;

    public async Task<RegisterOrganizerResult> Handle(
        RegisterOrganizerCommand request,
        CancellationToken cancellationToken)
    {
        var normalised = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == normalised, cancellationToken))
            throw new DuplicateEmailException();

        // Organizer user starts PendingVerification — must verify email before login (same as attendees)
        var user = new User
        {
            Email = normalised,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.ContactPerson.Trim(),
            Phone = request.Phone.Trim(),
            City = request.City.Trim(),
            Role = UserRole.Organizer,
            Status = UserStatus.PendingVerification
        };

        var rawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        user.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Token = rawToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });

        var org = new Organization
        {
            OrganizationName = request.OrgName.Trim(),
            ContactPerson = request.ContactPerson.Trim(),
            Category = request.Category.Trim(),
            Status = OrganizationStatus.PendingApproval,
            User = user
        };

        // Atomic: both User and Organization committed in one transaction
        db.Users.Add(user);
        db.Organizations.Add(org);
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

        return new RegisterOrganizerResult(user.Id, org.Id, user.Email, org.Status.ToString());
    }
}
