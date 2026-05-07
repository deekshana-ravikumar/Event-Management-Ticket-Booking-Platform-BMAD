using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers;

/// <summary>
/// Organizer-only stub — validates ActiveOrganizer policy enforcement (AC-E1-002-04).
/// Full implementation arrives in Sprint 2 (Events module).
/// </summary>
[ApiController]
[Route("api/organizer")]
[Authorize(Policy = "ActiveOrganizer")]
public sealed class OrganizerController : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult GetDashboard() =>
        Ok(new { message = "Organizer dashboard — Sprint 2 placeholder." });
}
