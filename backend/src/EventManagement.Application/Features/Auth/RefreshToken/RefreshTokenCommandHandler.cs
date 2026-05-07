using EventManagement.Application.Common.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    AppDbContext db,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = jwtTokenService.HashToken(request.RawRefreshToken);

        var stored = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (stored == null || !stored.IsActive)
            throw new InvalidRefreshTokenException();

        // Rotate: revoke old, issue new
        stored.RevokedAt = DateTime.UtcNow;

        var newRaw = jwtTokenService.GenerateRefreshTokenRaw();
        db.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = jwtTokenService.HashToken(newRaw),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await db.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(
            stored.User.Id, stored.User.Email, stored.User.Role.ToString());

        return new RefreshTokenResult(accessToken, 1800, newRaw);
    }
}
