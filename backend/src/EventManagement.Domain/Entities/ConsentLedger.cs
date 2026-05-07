using EventManagement.Domain.Enums;

namespace EventManagement.Domain.Entities;

public class ConsentLedger
{
    public long Id { get; set; }

    /// <summary>Null only for pre-registration cookie consent captured before user is created.</summary>
    public long? UserId { get; set; }

    public string? SessionId { get; set; }
    public ConsentType ConsentType { get; set; }
    public string? TncVersion { get; set; }

    /// <summary>IPv4 or IPv6. Stored for DPDP audit only — never logged to application logs.</summary>
    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
    public DateTime ConsentGivenAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
