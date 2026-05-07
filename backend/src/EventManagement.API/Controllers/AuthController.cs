using EventManagement.Application.Features.Auth.Login;
using EventManagement.Application.Features.Auth.Logout;
using EventManagement.Application.Features.Auth.RefreshToken;
using EventManagement.Application.Features.Auth.RegisterAttendee;
using EventManagement.Application.Features.Auth.RegisterOrganizer;
using EventManagement.Application.Features.Auth.ResendVerificationEmail;
using EventManagement.Application.Features.Auth.VerifyEmail;
using EventManagement.API.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EventManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender mediator, IOptions<CookieSettings> cookieSettings) : ControllerBase
{
    private readonly CookieSettings _cookieSettings = cookieSettings.Value;

    // POST /api/auth/register/attendee
    [HttpPost("register/attendee")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAttendee(
        [FromBody] RegisterAttendeeCommand command,
        CancellationToken cancellationToken)
    {
        var enriched = command with
        {
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await mediator.Send(enriched, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new
        {
            userId = result.UserId,
            email = result.Email,
            status = result.Status
        });
    }

    // POST /api/auth/register/organizer
    [HttpPost("register/organizer")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterOrganizer(
        [FromBody] RegisterOrganizerCommand command,
        CancellationToken cancellationToken)
    {
        var enriched = command with
        {
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await mediator.Send(enriched, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new
        {
            userId = result.UserId,
            orgId = result.OrgId,
            email = result.Email,
            status = result.OrgStatus
        });
    }

    // POST /api/auth/verify-email
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new VerifyEmailCommand(request.Token), cancellationToken);
        return Ok(new { message = "Email verified successfully." });
    }

    // POST /api/auth/resend-verification
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        // Always 200 regardless of whether email exists (anti-enumeration)
        await mediator.Send(new ResendVerificationEmailCommand(request.Email), cancellationToken);
        return Ok(new { message = "If your email is registered and unverified, a new verification email has been sent." });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var enriched = command with
        {
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await mediator.Send(enriched, cancellationToken);

        SetRefreshTokenCookie(result.RawRefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new
        {
            accessToken = result.AccessToken,
            expiresIn = result.ExpiresIn,
            userId = result.UserId,
            email = result.Email,
            role = result.Role,
            orgStatus = result.OrgStatus
        });
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(rawToken))
            return Unauthorized(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "Refresh token not found.",
                Type = "https://httpstatuses.io/401"
            });

        var result = await mediator.Send(new RefreshTokenCommand(rawToken), cancellationToken);

        SetRefreshTokenCookie(result.NewRawRefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new
        {
            accessToken = result.AccessToken,
            expiresIn = result.ExpiresIn
        });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            await mediator.Send(new LogoutCommand(userId), cancellationToken);

        ClearRefreshTokenCookie();

        return NoContent();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string rawToken, DateTime expires)
    {
        Response.Cookies.Append("refreshToken", rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = _cookieSettings.GetSameSiteMode(),
            Path = "/api/auth",
            Expires = expires
        });
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = _cookieSettings.GetSameSiteMode(),
            Path = "/api/auth"
        });
    }

    private string GetClientIpAddress()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// ─── Thin request DTOs (only used by endpoints that don't map directly to commands) ──

public record VerifyEmailRequest(string Token);
public record ResendVerificationRequest(string Email);
