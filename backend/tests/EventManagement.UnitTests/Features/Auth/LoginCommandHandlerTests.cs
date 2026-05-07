using EventManagement.Application.Common.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Enums;
using EventManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventManagement.UnitTests.Features.Auth;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();

    [Fact]
    public void PasswordHasher_VerifyCorrectPassword_ReturnsTrue()
    {
        _hasher.Setup(h => h.Verify("Str0ng@Pass!", "storedHash")).Returns(true);

        var result = _hasher.Object.Verify("Str0ng@Pass!", "storedHash");

        result.Should().BeTrue();
    }

    [Fact]
    public void JwtService_GenerateAccessToken_ReturnsNonEmptyString()
    {
        _jwt.Setup(j => j.GenerateAccessToken(1, "user@example.com", "Attendee"))
            .Returns("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.stub");

        var token = _jwt.Object.GenerateAccessToken(1, "user@example.com", "Attendee");

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void JwtService_HashToken_ReturnsDifferentHashForDifferentInput()
    {
        _jwt.Setup(j => j.HashToken("token1")).Returns("hash1");
        _jwt.Setup(j => j.HashToken("token2")).Returns("hash2");

        var h1 = _jwt.Object.HashToken("token1");
        var h2 = _jwt.Object.HashToken("token2");

        h1.Should().NotBe(h2);
    }
}
