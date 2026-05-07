using EventManagement.Domain.Enums;
using EventManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Middleware.Authorization;

public sealed class ActiveOrganizerRequirement : IAuthorizationRequirement { }

public sealed class ActiveOrganizerHandler(AppDbContext db)
    : AuthorizationHandler<ActiveOrganizerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveOrganizerRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            context.Fail();
            return;
        }

        // Always live DB lookup — never cache org status in JWT (design decision: no stale data risk)
        var org = await db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.UserId == userId);

        if (org?.Status == OrganizationStatus.Active)
            context.Succeed(requirement);
        else
            context.Fail(new AuthorizationFailureReason(this, "Organization is pending admin approval."));
    }
}
