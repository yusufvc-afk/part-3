
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class RegisterRequest
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "";

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}
