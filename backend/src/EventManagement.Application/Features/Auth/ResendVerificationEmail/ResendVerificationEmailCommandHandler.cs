using EventManagement.Application.Common.Interfaces;
using EventManagement.Application.Common.Settings;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Enums;
using EventManagement.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventManagement.Application.Features.Auth.ResendVerificationEmail;

public sealed class ResendVerificationEmailCommandHandler(
    AppDbContext db,
    IEmailService emailService,
    IOptions<FrontendSettings> frontendSettings)
    : IRequestHandler<ResendVerificationEmailCommand, Unit>
{
    private readonly FrontendSettings _frontend = frontendSettings.Value;

    public async Task<Unit> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var normalised = request.Email.Trim().ToLowerInvariant();

        // Anti-enumeration: always return success regardless of whether account exists
        var user = await db.Users
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.Email == normalised, cancellationToken);

        if (user == null || user.Status != UserStatus.PendingVerification)
            return Unit.Value; // Silent: no-op for non-existent or already-verified accounts

        // Invalidate all prior unexpired tokens for this user
        var priorTokens = user.EmailVerificationTokens
            .Where(t => t.UsedAt == null && DateTime.UtcNow < t.ExpiresAt)
            .ToList();

        foreach (var t in priorTokens)
            t.UsedAt = DateTime.UtcNow;

        // Issue new token
        var rawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        var newToken = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = rawToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        db.EmailVerificationTokens.Add(newToken);
        await db.SaveChangesAsync(cancellationToken);

        var verificationUrl = $"{_frontend.BaseUrl}/verify-email?token={Uri.EscapeDataString(rawToken)}";
        await emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationUrl, cancellationToken);

        return Unit.Value;
    }
}
