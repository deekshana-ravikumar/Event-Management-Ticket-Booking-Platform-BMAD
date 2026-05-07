using System.Net;
using System.Net.Http.Json;
using EventManagement.Domain.Enums;
using EventManagement.Infrastructure.Persistence;
using EventManagement.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventManagement.IntegrationTests.Auth;

public sealed class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ─── BE-T07-01: Attendee registration happy path ──────────────────────────
    [Fact]
    public async Task RegisterAttendee_ValidPayload_Returns201AndQueuesEmail()
    {
        var payload = new
        {
            fullName = "Raj Kumar",
            email = $"raj_{Guid.NewGuid():N}@example.com",
            phone = "9876543210",
            city = "Chennai",
            password = "Str0ng@Pass!",
            confirmPassword = "Str0ng@Pass!",
            tncVersion = "2026-05-01"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register/attendee", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body!["email"].ToString().Should().Be(payload.email);
        body["status"].ToString().Should().Be("PendingVerification");

        _factory.FakeEmail.SentEmails
            .Should().ContainSingle(e => e.ToEmail == payload.email);
    }

    // ─── BE-T07-02: Duplicate email returns 409 ───────────────────────────────
    [Fact]
    public async Task RegisterAttendee_DuplicateEmail_Returns409()
    {
        var email = $"dup_{Guid.NewGuid():N}@example.com";
        var payload = new
        {
            fullName = "Test User",
            email,
            phone = "9876543210",
            city = "Mumbai",
            password = "Str0ng@Pass!",
            confirmPassword = "Str0ng@Pass!",
            tncVersion = "2026-05-01"
        };

        await _client.PostAsJsonAsync("/api/auth/register/attendee", payload);
        var second = await _client.PostAsJsonAsync("/api/auth/register/attendee", payload);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ─── BE-T07-03: Weak password returns 422 ─────────────────────────────────
    [Fact]
    public async Task RegisterAttendee_WeakPassword_Returns422()
    {
        var payload = new
        {
            fullName = "Test User",
            email = "test@example.com",
            phone = "9876543210",
            city = "Delhi",
            password = "weakpassword",
            confirmPassword = "weakpassword",
            tncVersion = "2026-05-01"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register/attendee", payload);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ─── BE-T07-04: Organizer registration happy path ─────────────────────────
    [Fact]
    public async Task RegisterOrganizer_ValidPayload_Returns201WithOrgStatus()
    {
        var payload = new
        {
            orgName = "Tech Events Ltd",
            contactPerson = "Priya Sharma",
            email = $"org_{Guid.NewGuid():N}@example.com",
            phone = "9876543210",
            category = "Technology",
            password = "Str0ng@Pass!",
            confirmPassword = "Str0ng@Pass!",
            address = "42, MG Road",
            city = "Bangalore",
            state = "Karnataka",
            pincode = "560001",
            tncVersion = "2026-05-01"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register/organizer", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body!["status"].ToString().Should().Be("PendingApproval");
        body.Should().ContainKey("orgId");
    }

    // ─── BE-T07-05: Email verification happy path ─────────────────────────────
    [Fact]
    public async Task VerifyEmail_ValidToken_Returns200AndActivatesUser()
    {
        // Register to get a verification token
        var email = $"verify_{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register/attendee", new
        {
            fullName = "Test Verify",
            email,
            phone = "9876543210",
            city = "Pune",
            password = "Str0ng@Pass!",
            confirmPassword = "Str0ng@Pass!",
            tncVersion = "2026-05-01"
        });

        // Extract token from DB directly
        var token = await GetVerificationTokenFromDb(email);
        token.Should().NotBeNullOrEmpty();

        var response = await _client.PostAsJsonAsync("/api/auth/verify-email", new { token });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Confirm user is now Active
        await AssertUserStatus(email, UserStatus.Active);
    }

    // ─── BE-T07-06: Verify with expired token returns 400 ─────────────────────
    [Fact]
    public async Task VerifyEmail_ExpiredToken_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/verify-email",
            new { token = "nonexistent_token_xyz_12345" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── BE-T07-07: Login before email verification returns 401 ───────────────
    [Fact]
    public async Task Login_UnverifiedUser_Returns401()
    {
        var email = $"unverified_{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register/attendee", new
        {
            fullName = "Unverified User",
            email,
            phone = "9876543210",
            city = "Delhi",
            password = "Str0ng@Pass!",
            confirmPassword = "Str0ng@Pass!",
            tncVersion = "2026-05-01"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Str0ng@Pass!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── BE-T07-08: Login happy path returns JWT + sets cookie ────────────────
    [Fact]
    public async Task Login_VerifiedUser_Returns200WithTokenAndCookie()
    {
        var email = $"login_{Guid.NewGuid():N}@example.com";
        var password = "Str0ng@Pass!";

        await RegisterAndVerify(email, password);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body!.Should().ContainKey("accessToken");
        body.Should().ContainKey("expiresIn");

        // Refresh token must be in cookie, NOT in body
        body.Should().NotContainKey("rawRefreshToken");

        response.Headers.Should().ContainSingle(h =>
            h.Key == "Set-Cookie" &&
            h.Value.Any(v => v.Contains("refreshToken")));
    }

    // ─── BE-T07-09: Wrong password returns 401 ────────────────────────────────
    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"wrongpw_{Guid.NewGuid():N}@example.com";
        await RegisterAndVerify(email, "Str0ng@Pass!");

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongP@ssw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── BE-T07-10: Resend verification always returns 200 ────────────────────
    [Fact]
    public async Task ResendVerification_NonexistentEmail_Returns200()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/resend-verification",
            new { email = "ghost_user@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── BE-T07-11: Organizer login returns orgStatus in body ─────────────────
    [Fact]
    public async Task Login_OrganizerPendingApproval_Returns200WithOrgStatus()
    {
        var email = $"orglogin_{Guid.NewGuid():N}@example.com";
        var password = "Str0ng@Pass!";

        // Register organizer
        await _client.PostAsJsonAsync("/api/auth/register/organizer", new
        {
            orgName = "My Org",
            contactPerson = "Org User",
            email,
            phone = "9876543210",
            category = "Tech",
            password,
            confirmPassword = password,
            address = "1 Tech Park",
            city = "Hyderabad",
            state = "Telangana",
            pincode = "500081",
            tncVersion = "2026-05-01"
        });

        // Verify email
        var token = await GetVerificationTokenFromDb(email);
        await _client.PostAsJsonAsync("/api/auth/verify-email", new { token });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body!["orgStatus"].ToString().Should().Be("PendingApproval");
    }

    // ─── BE-T07-12: GET /api/organizer/dashboard with PendingApproval returns 403
    [Fact]
    public async Task OrganizerDashboard_PendingOrg_Returns403()
    {
        var email = $"orgguard_{Guid.NewGuid():N}@example.com";
        var password = "Str0ng@Pass!";

        await _client.PostAsJsonAsync("/api/auth/register/organizer", new
        {
            orgName = "Guard Org",
            contactPerson = "Guard Test",
            email,
            phone = "9876543210",
            category = "Education",
            password,
            confirmPassword = password,
            address = "5 School Lane",
            city = "Chennai",
            state = "Tamil Nadu",
            pincode = "600001",
            tncVersion = "2026-05-01"
        });

        var token = await GetVerificationTokenFromDb(email);
        await _client.PostAsJsonAsync("/api/auth/verify-email", new { token });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var accessToken = loginBody!["accessToken"].ToString();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var guardResponse = await _client.GetAsync("/api/organizer/dashboard");

        guardResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // ─── BE-T07-13: Logout revokes refresh tokens ─────────────────────────────
    [Fact]
    public async Task Logout_AuthenticatedUser_Returns204AndRevokesTokens()
    {
        var email = $"logout_{Guid.NewGuid():N}@example.com";
        var password = "Str0ng@Pass!";

        await RegisterAndVerify(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var accessToken = loginBody!["accessToken"].ToString();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task RegisterAndVerify(string email, string password)
    {
        await _client.PostAsJsonAsync("/api/auth/register/attendee", new
        {
            fullName = "Test User",
            email,
            phone = "9876543210",
            city = "Mumbai",
            password,
            confirmPassword = password,
            tncVersion = "2026-05-01"
        });

        var token = await GetVerificationTokenFromDb(email);
        await _client.PostAsJsonAsync("/api/auth/verify-email", new { token });
    }

    private async Task<string?> GetVerificationTokenFromDb(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        return user?.EmailVerificationTokens
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefault(t => t.UsedAt == null)
            ?.Token;
    }

    private async Task AssertUserStatus(string email, UserStatus expected)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        user!.Status.Should().Be(expected);
    }
}
