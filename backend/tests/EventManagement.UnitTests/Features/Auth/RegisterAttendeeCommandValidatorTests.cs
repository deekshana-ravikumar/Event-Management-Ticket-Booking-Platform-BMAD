using EventManagement.Application.Features.Auth.RegisterAttendee;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace EventManagement.UnitTests.Features.Auth;

public sealed class RegisterAttendeeCommandValidatorTests
{
    private readonly RegisterAttendeeCommandValidator _sut = new();

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoSpecialChar1")]
    [InlineData("No1!abc")]          // too short (7 chars)
    public void Validate_WeakPassword_HasError(string password)
    {
        var cmd = ValidCommand() with { Password = password, ConfirmPassword = password };
        var result = _sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordMismatch_HasError()
    {
        var cmd = ValidCommand() with { Password = "Str0ng@Pass!", ConfirmPassword = "Different@Pass1!" };
        var result = _sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Validate_InvalidEmail_HasError()
    {
        var cmd = ValidCommand() with { Email = "not-an-email" };
        var result = _sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_PhoneNotAllDigits_HasError()
    {
        var cmd = ValidCommand() with { Phone = "98765ABC10" };
        var result = _sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var cmd = ValidCommand();
        var result = _sut.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static RegisterAttendeeCommand ValidCommand() => new()
    {
        FullName = "Raj Kumar",
        Email = "raj@example.com",
        Phone = "9876543210",
        City = "Chennai",
        Password = "Str0ng@Pass!",
        ConfirmPassword = "Str0ng@Pass!",
        TncVersion = "2026-05-01"
    };
}
