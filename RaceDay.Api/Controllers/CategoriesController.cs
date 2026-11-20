
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDb _db;

    public CategoriesController(RaceDayDb db)
    {
        _db = db;
    }

    // VIEW ALL CATEGORIES
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                c.Description
            })
            .ToListAsync();

        return Ok(categories);
    }

    // CREATE A CATEGORY
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request)
    {
        var name = request.CategoryName?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        bool exists = await _db.Categories
            .AnyAsync(c => c.CategoryName == name);

        if (exists)
        {
            return Conflict(new
            {
                message = "Category already exists."
            });
        }

        var category = new Category
        {
            CategoryName = name,
            Description = request.Description?.Trim()
        };

        _db.Categories.Add(category);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _db.Categories.AsNoTracking()
                .AnyAsync(c => c.CategoryName == name))
            {
                return Conflict(new
                {
                    message = "Category already exists."
                });
            }

            throw;
        }

        return StatusCode(201, new
        {
            message = "Category created successfully.",
            category.CategoryId,
            category.CategoryName
        });
    }

    // ASSIGN CATEGORY TO EVENT
    [Authorize(Roles = "Organiser")]
    [HttpPost("/api/events/{eventId:int}/categories")]
    public async Task<IActionResult> AssignToEvent(
        int eventId,
        [FromBody] AssignCategoryRequest request)
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

        bool categoryExists = await _db.Categories
            .AnyAsync(c => c.CategoryId == request.CategoryId);

        if (!categoryExists)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        bool alreadyAssigned = await _db.EventCategories
            .AnyAsync(ec =>
                ec.EventId == eventId &&
                ec.CategoryId == request.CategoryId);

        if (alreadyAssigned)
        {
            return Conflict(new
            {
                message = "Category already assigned to this event."
            });
        }

        var eventCategory = new EventCategory
        {
            EventId = eventId,
            CategoryId = request.CategoryId,
            EntryFee = request.EntryFee,
            MaximumParticipants = request.MaximumParticipants
        };

        _db.EventCategories.Add(eventCategory);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _db.EventCategories.AsNoTracking()
                .AnyAsync(ec =>
                    ec.EventId == eventId &&
                    ec.CategoryId == request.CategoryId))
            {
                return Conflict(new
                {
                    message = "Category already assigned to this event."
                });
            }

            throw;
        }

        return StatusCode(201, new
        {
            message = "Category assigned successfully.",
            eventCategory.EventCategoryId,
            eventCategory.EventId,
            eventCategory.CategoryId,
            eventCategory.EntryFee,
            eventCategory.MaximumParticipants
        });
    }

    // VIEW CATEGORIES FOR AN EVENT
    [AllowAnonymous]
    [HttpGet("/api/events/{eventId:int}/categories")]
    public async Task<IActionResult> GetEventCategories(int eventId)
    {
        bool eventExists = await _db.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
            return NotFound(new { message = "Event not found." });

        var categories = await _db.EventCategories
            .AsNoTracking()
            .Where(ec => ec.EventId == eventId)
            .Select(ec => new
            {
                ec.EventCategoryId,
                ec.EventId,
                ec.CategoryId,
                ec.Category.CategoryName,
                ec.Category.Description,
                ec.EntryFee,
                ec.MaximumParticipants
            })
            .ToListAsync();

        return Ok(categories);
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
