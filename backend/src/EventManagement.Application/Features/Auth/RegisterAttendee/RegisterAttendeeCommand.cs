using MediatR;

namespace EventManagement.Application.Features.Auth.RegisterAttendee;

public record RegisterAttendeeCommand : IRequest<RegisterAttendeeResult>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public string? TncVersion { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public record RegisterAttendeeResult(long UserId, string Email, string Status);
