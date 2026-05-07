namespace EventManagement.Domain.Entities;

/// <summary>
/// Records every login attempt for lockout tracking (US-E1-006).
/// Email stored normalised (lowercase). IpAddress stored for audit only.
/// </summary>
public class LoginAttempt
{
    public long Id { get; set; }

    /// <summary>Normalised (lowercase) email — NOT a foreign key, to record attempts against non-existent accounts.</summary>
    public string Email { get; set; } = string.Empty;

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public bool Succeeded { get; set; }
    public string? IpAddress { get; set; }
}
