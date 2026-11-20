
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class UpdateProfileRequest
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = "";

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}
