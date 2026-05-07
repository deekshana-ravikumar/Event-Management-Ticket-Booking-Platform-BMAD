using EventManagement.Application.Common.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Enums;
using EventManagement.Domain.Exceptions;
using EventManagement.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalised = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalised, cancellationToken);

        var attempt = new LoginAttempt
        {
            Email = normalised,
            Succeeded = false,
            IpAddress = request.IpAddress,
            AttemptedAt = DateTime.UtcNow
        };

        // Constant-time check — same response for unknown email or wrong password (AC-E1-004-03 anti-enumeration)
        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            db.LoginAttempts.Add(attempt);
            await db.SaveChangesAsync(cancellationToken);
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        if (user.Status == UserStatus.PendingVerification)
            throw new AuthenticationFailedException("Please verify your email before logging in.");

        if (user.Status is UserStatus.Suspended or UserStatus.Deactivated)
            throw new AuthenticationFailedException("Your account is not active. Please contact support.");

        attempt.Succeeded = true;
        db.LoginAttempts.Add(attempt);

        var rawRefreshToken = jwtTokenService.GenerateRefreshTokenRaw();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = jwtTokenService.HashToken(rawRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await db.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());

        string? orgStatus = null;
        if (user.Role == UserRole.Organizer)
        {
            var org = await db.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == user.Id, cancellationToken);
            orgStatus = org?.Status.ToString();
        }

        return new LoginResult(accessToken, 1800, user.Id, user.Email, user.Role.ToString(), orgStatus, rawRefreshToken);
    }
}
