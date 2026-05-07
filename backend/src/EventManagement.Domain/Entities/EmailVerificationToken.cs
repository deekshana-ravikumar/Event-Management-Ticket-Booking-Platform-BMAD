namespace EventManagement.Domain.Entities;

public class EmailVerificationToken
{
    public long Id { get; set; }
    public long UserId { get; set; }

    /// <summary>Cryptographically random token. NOT a hash — the raw value is emailed.</summary>
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public bool IsValid => UsedAt == null && DateTime.UtcNow < ExpiresAt;
}
