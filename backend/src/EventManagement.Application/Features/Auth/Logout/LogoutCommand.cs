using MediatR;

namespace EventManagement.Application.Features.Auth.Logout;

public record LogoutCommand(long UserId) : IRequest<Unit>;
