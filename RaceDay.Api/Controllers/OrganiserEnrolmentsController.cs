
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Organiser")]
[Route("api/events")]
public class OrganiserEnrolmentsController : ControllerBase
{
    private readonly RaceDayDb _db;

    public OrganiserEnrolmentsController(RaceDayDb db)
    {
        _db = db;
    }

    // VIEW ALL ENROLMENTS FOR MY EVENT
    [HttpGet("{eventId:int}/enrolments")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var raceEvent = await _db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (raceEvent == null)
            return NotFound(new { message = "Event not found." });

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();

        var enrolments = await _db.Enrolments
            .AsNoTracking()
            .Where(e => e.EventCategory.EventId == eventId)
            .OrderBy(e => e.EventCategory.Category.CategoryName)
            .ThenBy(e => e.Participant.LastName)
            .Select(e => new
            {
                e.EnrolmentId,
                e.EnrolmentDate,
                e.Status,
                e.ParticipantId,
                e.Participant.FirstName,
                e.Participant.LastName,
                e.Participant.Email,
                e.EventCategoryId,
                e.EventCategory.Category.CategoryName
            })
            .ToListAsync();

        return Ok(new
        {
            raceEvent.EventId,
            raceEvent.EventName,
            totalEnrolments = enrolments.Count,
            activeEnrolments = enrolments.Count(
                e => e.Status != "Cancelled"),
            enrolments
        });
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
