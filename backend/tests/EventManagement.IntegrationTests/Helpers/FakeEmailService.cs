using EventManagement.Application.Common.Interfaces;
using EventManagement.Domain.Enums;

namespace EventManagement.IntegrationTests.Helpers;

/// <summary>
/// Captures emails sent during integration tests — no SMTP connection required.
/// </summary>
public sealed class FakeEmailService : IEmailService
{
    private readonly List<SentEmail> _sent = new();

    public IReadOnlyList<SentEmail> SentEmails => _sent.AsReadOnly();

    public Task SendVerificationEmailAsync(
        string toEmail,
        string fullName,
        string verificationUrl,
        CancellationToken cancellationToken = default)
    {
        _sent.Add(new SentEmail(toEmail, fullName, verificationUrl));
        return Task.CompletedTask;
    }

    public void Clear() => _sent.Clear();
}

public record SentEmail(string ToEmail, string FullName, string VerificationUrl);
