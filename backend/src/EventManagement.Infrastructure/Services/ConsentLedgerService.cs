using EventManagement.Application.Common.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Enums;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Services;

public sealed class ConsentLedgerService(AppDbContext db) : IConsentLedgerService
{
    public async Task RecordConsentAsync(
        long? userId,
        ConsentType consentType,
        string? tncVersion,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        db.ConsentLedger.Add(new ConsentLedger
        {
            UserId = userId,
            ConsentType = consentType,
            TncVersion = tncVersion,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ConsentGivenAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
