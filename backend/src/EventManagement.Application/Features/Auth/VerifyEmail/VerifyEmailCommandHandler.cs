using EventManagement.Domain.Enums;
using EventManagement.Domain.Exceptions;
using EventManagement.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Auth.VerifyEmail;

public sealed class VerifyEmailCommandHandler(AppDbContext db)
    : IRequestHandler<VerifyEmailCommand, Unit>
{
    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var token = await db.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (token == null)
            throw new VerificationTokenExpiredException();

        if (token.UsedAt != null)
            throw new InvalidVerificationTokenException();

        if (DateTime.UtcNow >= token.ExpiresAt)
            throw new VerificationTokenExpiredException();

        token.UsedAt = DateTime.UtcNow;
        token.User.Status = UserStatus.Active;
        token.User.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
