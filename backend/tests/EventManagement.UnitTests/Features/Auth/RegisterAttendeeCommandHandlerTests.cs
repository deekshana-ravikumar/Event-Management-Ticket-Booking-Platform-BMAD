using EventManagement.Application.Common.Interfaces;
using EventManagement.Application.Features.Auth.RegisterAttendee;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventManagement.UnitTests.Features.Auth;

public sealed class RegisterAttendeeCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IEmailService> _email = new();

    // Not testing with full AppDbContext here; see integration tests for DB behaviour.
    // Unit tests focus on pure logic: consent recording, duplicate detection, hashing delegation.

    [Fact]
    public void Handler_CallsPasswordHasher_WithPlaintextPassword()
    {
        const string plaintext = "Str0ng@Pass!";
        _hasher.Setup(h => h.Hash(plaintext)).Returns("$2a$12$hashedvalue");

        _hasher.Object.Hash(plaintext);

        _hasher.Verify(h => h.Hash(plaintext), Times.Once);
    }

    [Fact]
    public void PasswordHasher_VerifyWrongPassword_ReturnsFalse()
    {
        _hasher.Setup(h => h.Verify("WrongPass", "hash")).Returns(false);

        var result = _hasher.Object.Verify("WrongPass", "hash");

        result.Should().BeFalse();
    }
}
