namespace EventManagement.Application.Common.Settings;

public sealed class JwtSettings
{
    public string Secret { get; set; } = string.Empty; // Populated from JWT_SECRET env var in Program.cs
    public string Issuer { get; set; } = "EventManagement";
    public string Audience { get; set; } = "EventManagement";
    public int AccessTokenExpiryMinutes { get; set; } = 30;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
