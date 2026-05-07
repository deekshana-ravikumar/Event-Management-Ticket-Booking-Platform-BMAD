using System.Text.Encodings.Web;
using EventManagement.API.Configuration;
using EventManagement.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventManagement.Infrastructure.Services;

public sealed class EmailService(
    IOptions<SmtpSettings> settings,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = settings.Value;

    public async Task SendVerificationEmailAsync(
        string toEmail,
        string fullName,
        string verificationUrl,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "Verify your email — Event Platform";

        message.Body = new TextPart("html")
        {
            Text = BuildVerificationEmailBody(fullName, verificationUrl)
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            // Log by userId reference only — never log email address (PII)
            logger.LogInformation("Verification email dispatched successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email (SMTP error)");
            throw;
        }
    }

    private static string BuildVerificationEmailBody(string fullName, string verificationUrl)
    {
        var encodedName = HtmlEncoder.Default.Encode(fullName);
        var encodedUrl = HtmlEncoder.Default.Encode(verificationUrl);

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
              <h2 style="color: #1E40AF;">Welcome to Event Platform!</h2>
              <p>Hi {encodedName},</p>
              <p>Please verify your email address by clicking the button below:</p>
              <p style="margin: 30px 0;">
                <a href="{encodedUrl}"
                   style="background-color: #1E40AF; color: white; padding: 12px 24px;
                          text-decoration: none; border-radius: 6px; font-weight: bold;">
                  Verify Email
                </a>
              </p>
              <p>Or copy this link: <a href="{encodedUrl}">{encodedUrl}</a></p>
              <p><strong>This link expires in 24 hours.</strong></p>
              <p style="color: #64748B; font-size: 13px;">
                If you did not create an account on Event Platform, please ignore this email.
              </p>
            </body>
            </html>
            """;
    }
}
