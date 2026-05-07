using MediatR;

namespace EventManagement.Application.Features.Auth.ResendVerificationEmail;

public record ResendVerificationEmailCommand(string Email) : IRequest<Unit>;
