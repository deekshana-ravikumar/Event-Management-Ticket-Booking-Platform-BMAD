using EventManagement.Domain.Enums;

namespace EventManagement.Application.Common.Interfaces;

public interface IConsentLedgerService
{
    Task RecordConsentAsync(
        long? userId,
        ConsentType consentType,
        string? tncVersion,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);
}
