using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests
{
    private readonly PasswordHasher<User> _hasher = new();

    [Fact]
    public void PasswordHash_IsNotPlainText()
    {
        var user = new User
        {
            Email = "test@raceday.co.za"
        };

        const string password = "RaceDay2026!";

        string hash = _hasher.HashPassword(user, password);

        Assert.NotEqual(password, hash);
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void CorrectPassword_VerifiesSuccessfully()
    {
        var user = new User
        {
            Email = "test@raceday.co.za"
        };

        string hash = _hasher.HashPassword(
            user, "RaceDay2026!");

        var result = _hasher.VerifyHashedPassword(
            user, hash, "RaceDay2026!");

        Assert.NotEqual(
            PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void IncorrectPassword_IsRejected()
    {
        var user = new User
        {
            Email = "test@raceday.co.za"
        };

        string hash = _hasher.HashPassword(
            user, "RaceDay2026!");

        var result = _hasher.VerifyHashedPassword(
            user, hash, "WrongPassword!");

        Assert.Equal(
            PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void OrganiserIdentity_HasOrganiserRole()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Organiser")
            },
            "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        Assert.True(principal.Identity!.IsAuthenticated);
        Assert.True(principal.IsInRole("Organiser"));
        Assert.False(principal.IsInRole("Participant"));
    }

    [Fact]
    public void ParticipantIdentity_HasParticipantRole()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Role, "Participant")
            },
            "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        Assert.True(principal.IsInRole("Participant"));
        Assert.False(principal.IsInRole("Organiser"));
    }
}
