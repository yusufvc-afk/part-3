
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDb _db;

    public ResultsController(RaceDayDb db)
    {
        _db = db;
    }

    // ORGANISER RECORDS A RESULT
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> Record(
        [FromBody] RecordResultRequest request)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        if (request.FinishTime == null ||
            request.FinishTime.Value <= TimeSpan.Zero ||
            request.FinishTime.Value >= TimeSpan.FromDays(1))
        {
            return BadRequest(new
            {
                message = "Finish time must be greater than zero and less than 24 hours."
            });
        }

        var enrolment = await _db.Enrolments
            .Include(e => e.EventCategory)
            .ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(e =>
                e.EnrolmentId == request.EnrolmentId);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        var raceEvent = enrolment.EventCategory.Event;

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();

        if (raceEvent.Status != "Completed")
        {
            return Conflict(new
            {
                message = "The event must be marked Completed before recording results."
            });
        }

        if (enrolment.Status == "Cancelled")
        {
            return Conflict(new
            {
                message = "Cannot record a result for a cancelled enrolment."
            });
        }

        bool resultExists = await _db.Results
            .AnyAsync(r => r.EnrolmentId == request.EnrolmentId);

        if (resultExists)
        {
            return Conflict(new
            {
                message = "A result already exists for this enrolment."
            });
        }

        bool positionTaken = await _db.Results
            .AnyAsync(r =>
                r.Enrolment.EventCategoryId ==
                    enrolment.EventCategoryId &&
                r.Position == request.Position);

        if (positionTaken)
        {
            return Conflict(new
            {
                message = "This finishing position is already assigned in the category."
            });
        }

        var result = new RaceResult
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = request.FinishTime.Value,
            Position = request.Position,
            RecordedAt = DateTime.UtcNow
        };

        _db.Results.Add(result);
        enrolment.Status = "Completed";

        await _db.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Result recorded successfully.",
            result.ResultId,
            result.EnrolmentId,
            result.FinishTime,
            result.Position
        });
    }

    // PARTICIPANT VIEWS OWN RESULTS
    [Authorize(Roles = "Participant")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyResults()
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var results = await _db.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment.ParticipantId == participantId)
            .OrderByDescending(r =>
                r.Enrolment.EventCategory.Event.EventDate)
            .Select(r => new
            {
                r.ResultId,
                r.FinishTime,
                r.Position,
                r.RecordedAt,
                r.Enrolment.EventCategory.Event.EventName,
                r.Enrolment.EventCategory.Event.EventDate,
                r.Enrolment.EventCategory.Category.CategoryName
            })
            .ToListAsync();

        return Ok(results);
    }

    // ORGANISER VIEWS RESULTS FOR OWN EVENT
    [Authorize(Roles = "Organiser")]
    [HttpGet("/api/events/{eventId:int}/results")]
    public async Task<IActionResult> GetEventResults(int eventId)
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

        var results = await _db.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment.EventCategory.EventId == eventId)
            .OrderBy(r =>
                r.Enrolment.EventCategory.Category.CategoryName)
            .ThenBy(r => r.Position)
            .Select(r => new
            {
                r.ResultId,
                r.EnrolmentId,
                r.Position,
                r.FinishTime,
                r.Enrolment.Participant.FirstName,
                r.Enrolment.Participant.LastName,
                r.Enrolment.EventCategory.Category.CategoryName
            })
            .ToListAsync();

        return Ok(new
        {
            raceEvent.EventId,
            raceEvent.EventName,
            results
        });
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
