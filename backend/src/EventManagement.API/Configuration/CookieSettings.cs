using Microsoft.AspNetCore.Http;

namespace EventManagement.API.Configuration;

public sealed class CookieSettings
{
    public string SameSite { get; set; } = "Strict";

    public SameSiteMode GetSameSiteMode() => SameSite.ToLowerInvariant() switch
    {
        "lax" => SameSiteMode.Lax,
        "none" => SameSiteMode.None,
        _ => SameSiteMode.Strict
    };
}
