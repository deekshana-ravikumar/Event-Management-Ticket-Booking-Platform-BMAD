using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ConsentLedger> ConsentLedger { get; }
    DbSet<LoginAttempt> LoginAttempts { get; }
    DbSet<Organization> Organizations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
