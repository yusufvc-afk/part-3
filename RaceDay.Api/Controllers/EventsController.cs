using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDb _db;

    public EventsController(RaceDayDb db)
    {
        _db = db;
    }

    // GET ALL EVENTS
    /// <summary>
    /// Returns all RaceDay events.
    /// </summary>
    /// <remarks>
    /// This endpoint is publicly accessible.
    /// </remarks>
    /// <response code="200">Events retrieved successfully.</response>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events
            .AsNoTracking()
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventId,
                e.EventName,
                e.Description,
                e.EventDate,
                e.StartTime,
                e.Location,
                e.DistanceKm,
                e.EventType,
                e.Status,
                e.OrganiserId
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET ONE EVENT
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var raceEvent = await _db.Events
            .AsNoTracking()
            .Where(e => e.EventId == id)
            .Select(e => new
            {
                e.EventId,
                e.EventName,
                e.Description,
                e.EventDate,
                e.StartTime,
                e.Location,
                e.DistanceKm,
                e.EventType,
                e.Status,
                e.OrganiserId
            })
            .FirstOrDefaultAsync();

        if (raceEvent == null)
            return NotFound(new { message = "Event not found." });

        return Ok(raceEvent);
    }

    // CREATE EVENT
    /// <summary>
    /// Creates a new RaceDay event.
    /// </summary>
    /// <remarks>
    /// Requires an authenticated Organiser.
    /// </remarks>
    /// <response code="201">Event created.</response>
    /// <response code="400">Invalid event details.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not an Organiser.</response>
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventRequest request)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var error = ValidateEvent(request);
        if (error != null)
            return BadRequest(new { message = error });
        if (request.EventDate!.Value.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new
            {
                message = "New events cannot have a past date."
            });
        }

        var raceEvent = new RaceEvent
        {
            OrganiserId = organiserId,
            EventName = request.EventName.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate!.Value.Date,
            StartTime = request.StartTime!.Value,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = request.EventType.Trim(),
            Status = "Planned",
            CreatedAt = DateTime.UtcNow
        };

        _db.Events.Add(raceEvent);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = raceEvent.EventId },
            new
            {
                message = "Event created successfully.",
                raceEvent.EventId,
                raceEvent.EventName,
                raceEvent.EventType,
                raceEvent.Status
            });
    }

    // UPDATE EVENT
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEventRequest request)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var raceEvent = await _db.Events
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (raceEvent == null)
            return NotFound(new { message = "Event not found." });

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();


        if (raceEvent.Status == "Completed")
        {
            return Conflict(new ApiError(
                "Completed events cannot be modified."));
        }


        var error = ValidateEvent(request);
        if (error != null)
            return BadRequest(new { message = error });

        var status = request.Status?.Trim();

        if (status != "Planned" &&
            status != "Open" &&
            status != "Closed" &&
            status != "Completed")
        {
            return BadRequest(new
            {
                message = "Invalid event status."
            });
        }

        raceEvent.EventName = request.EventName.Trim();
        raceEvent.Description = request.Description?.Trim();
        raceEvent.EventDate = request.EventDate!.Value.Date;
        raceEvent.StartTime = request.StartTime!.Value;
        raceEvent.Location = request.Location.Trim();
        raceEvent.DistanceKm = request.DistanceKm;
        raceEvent.EventType = request.EventType.Trim();
        raceEvent.Status = status!;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Event updated successfully.",
            raceEvent.EventId,
            raceEvent.EventName,
            raceEvent.Status
        });
    }

    // DELETE EVENT

    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var raceEvent = await _db.Events
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (raceEvent == null)
            return NotFound(new { message = "Event not found." });

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();

        // Prevent deleting completed events
        if (raceEvent.Status == "Completed")
        {
            return Conflict(new ApiError(
                "Completed events cannot be deleted."));
        }

        // Prevent deleting events with assigned categories
        bool hasCategories = await _db.EventCategories
            .AnyAsync(ec => ec.EventId == id);

        if (hasCategories)
        {
            return Conflict(new ApiError(
                "Cannot delete an event with assigned categories."));
        }

        _db.Events.Remove(raceEvent);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static string? ValidateEvent(
        CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventName) ||
            string.IsNullOrWhiteSpace(request.Location))
            return "Event name and location are required.";

        if (request.EventDate == null)
            return "Event date is required.";

        if (request.StartTime == null ||
            request.StartTime.Value < TimeSpan.Zero ||
            request.StartTime.Value >= TimeSpan.FromDays(1))
            return "A valid start time is required.";

        if (request.DistanceKm <= 0)
            return "Distance must be greater than zero.";

        var type = request.EventType?.Trim();

        if (type != "Run" && type != "Walk" && type != "Cycle")
            return "Event type must be Run, Walk or Cycle.";

        return null;
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
