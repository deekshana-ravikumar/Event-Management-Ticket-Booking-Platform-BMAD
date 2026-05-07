using MediatR;

namespace EventManagement.Application.Features.Auth.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<Unit>;
