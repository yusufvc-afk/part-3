using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Participant")]
[Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDb _db;

    public EnrolmentsController(RaceDayDb db)
    {
        _db = db;
    }

    // PARTICIPANT ENROLS IN AN EVENT CATEGORY
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] EnrolmentRequest request)
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var eventCategory = await _db.EventCategories
            .Include(ec => ec.Event)
            .FirstOrDefaultAsync(ec =>
                ec.EventCategoryId == request.EventCategoryId);

        if (eventCategory == null)
            return NotFound(new { message = "Event category not found." });

        if (eventCategory.Event.Status != "Open")
        {
            return Conflict(new
            {
                message = "This event is not open for enrolment."
            });
        }

        if (eventCategory.Event.EventDate.Date < DateTime.UtcNow.Date)
        {
            return Conflict(new
            {
                message = "This event has already passed."
            });
        }

        // Prevent enrolling twice in the same event
        bool alreadyEnrolled = await _db.Enrolments
            .AnyAsync(e =>
                e.ParticipantId == participantId &&
                e.EventCategory.EventId == eventCategory.EventId &&
                e.Status != "Cancelled");

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "You are already enrolled in this event."
            });
        }

        int activeCount = await _db.Enrolments
            .CountAsync(e =>
                e.EventCategoryId == request.EventCategoryId &&
                e.Status != "Cancelled");

        if (activeCount >= eventCategory.MaximumParticipants)
        {
            return Conflict(new
            {
                message = "This event category is full."
            });
        }

        // Reuse an existing cancelled enrolment for this category
        var enrolment = await _db.Enrolments
            .FirstOrDefaultAsync(e =>
                e.ParticipantId == participantId &&
                e.EventCategoryId == request.EventCategoryId);

        if (enrolment != null)
        {
            if (enrolment.Status != "Cancelled")
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this category."
                });
            }

            enrolment.Status = "Registered";
            enrolment.EnrolmentDate = DateTime.UtcNow;
        }
        else
        {
            enrolment = new Enrolment
            {
                ParticipantId = participantId,
                EventCategoryId = request.EventCategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Registered"
            };

            _db.Enrolments.Add(enrolment);
        }

        await _db.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Enrolment successful.",
            enrolment.EnrolmentId,
            enrolment.EventCategoryId,
            enrolment.Status
        });
    }

    // VIEW MY ENROLMENTS
    [HttpGet("my")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var enrolments = await _db.Enrolments
            .AsNoTracking()
            .Where(e => e.ParticipantId == participantId)
            .OrderByDescending(e => e.EnrolmentDate)
            .Select(e => new
            {
                e.EnrolmentId,
                e.Status,
                e.EnrolmentDate,
                e.EventCategoryId,
                e.EventCategory.Event.EventId,
                e.EventCategory.Event.EventName,
                e.EventCategory.Event.EventDate,
                e.EventCategory.Category.CategoryName
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // CANCEL MY ENROLMENT
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var enrolment = await _db.Enrolments
            .Include(e => e.EventCategory)
            .ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        if (enrolment.ParticipantId != participantId)
            return Forbid();


        bool hasResult = await _db.Results
            .AnyAsync(r => r.EnrolmentId == enrolment.EnrolmentId);

        if (hasResult)
        {
            return Conflict(new ApiError(
                "An enrolment with a recorded result cannot be cancelled."));
        }


        if (enrolment.Status == "Cancelled")
        {
            return Conflict(new
            {
                message = "Enrolment is already cancelled."
            });
        }

        if (enrolment.Status == "Completed" ||
            enrolment.EventCategory.Event.EventDate.Date < DateTime.UtcNow.Date)
        {
            return Conflict(new
            {
                message = "This enrolment can no longer be cancelled."
            });
        }

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Enrolment cancelled.",
            enrolment.EnrolmentId,
            enrolment.Status
        });
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
