
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDb _db;

    public ProfileController(RaceDayDb db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new
            {
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.PhoneNumber,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var firstName = request.FirstName?.Trim() ?? "";
        var lastName = request.LastName?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            return BadRequest(new
            {
                message = "First name and last name are required."
            });
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return Unauthorized();

        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = request.PhoneNumber?.Trim();

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully.",
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.PhoneNumber
        });
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
