
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/role-access")]
public class RoleAccessController : ControllerBase
{
    [Authorize(Roles = "Organiser")]
    [HttpGet("organiser")]
    public IActionResult OrganiserOnly()
    {
        return Ok(new
        {
            message = "Organiser access granted."
        });
    }

    [Authorize(Roles = "Participant")]
    [HttpGet("participant")]
    public IActionResult ParticipantOnly()
    {
        return Ok(new
        {
            message = "Participant access granted."
        });
    }
}
