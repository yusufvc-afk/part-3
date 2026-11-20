
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDb _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        RaceDayDb db,
        IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // REGISTER
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var firstName = request.FirstName?.Trim() ?? "";
        var lastName = request.LastName?.Trim() ?? "";
        var email = request.Email?.Trim().ToLowerInvariant() ?? "";
        var role = request.Role?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            return BadRequest(new
            {
                message = "First name and last name are required."
            });
        }

        if (role != "Organiser" && role != "Participant")
        {
            return BadRequest(new
            {
                message = "Role must be Organiser or Participant."
            });
        }

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role,
            PhoneNumber = request.PhoneNumber?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _db.Users.AsNoTracking()
                .AnyAsync(u => u.Email == email))
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            throw;
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Registration successful.",
            userId = user.UserId,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            role = user.Role
        });
    }

    // LOGIN
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(user, request.Password);

            await _db.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(ClaimTypes.Name, user.Email),

            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            });

        return Ok(new
        {
            message = "Login successful.",
            userId = user.UserId,
            email = user.Email,
            role = user.Role
        });
    }

    // LOGOUT
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new
        {
            message = "Logout successful."
        });
    }

    // CURRENT USER
    [Authorize]
    [HttpGet("me")]
    public IActionResult CurrentUser()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier),

            email = User.FindFirstValue(
                ClaimTypes.Name),

            role = User.FindFirstValue(
                ClaimTypes.Role)
        });
    }
}
