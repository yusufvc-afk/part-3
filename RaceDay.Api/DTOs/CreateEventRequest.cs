
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class CreateEventRequest
{
    [Required]
    [StringLength(150)]
    public string EventName { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime? EventDate { get; set; }

    [Required]
    public TimeSpan? StartTime { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = "";

    [Range(0.01, 9999.99)]
    public decimal DistanceKm { get; set; }

    [Required]
    public string EventType { get; set; } = "";
}
