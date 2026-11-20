
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class UpdateEventRequest : CreateEventRequest
{
    [Required]
    public string Status { get; set; } = "";
}
