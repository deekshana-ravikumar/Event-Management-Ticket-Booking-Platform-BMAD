using MediatR;

namespace EventManagement.Application.Features.Auth.RegisterOrganizer;

public record RegisterOrganizerCommand : IRequest<RegisterOrganizerResult>
{
    public string OrgName { get; init; } = string.Empty;
    public string ContactPerson { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Pincode { get; init; } = string.Empty;
    public string? Website { get; init; }
    public string? TncVersion { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public record RegisterOrganizerResult(long UserId, long OrgId, string Email, string OrgStatus);
