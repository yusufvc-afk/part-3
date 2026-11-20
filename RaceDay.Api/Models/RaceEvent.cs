
namespace RaceDay.Api.Models;

public class RaceEvent
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "Run";
    public string Status { get; set; } = "Planned";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Organiser { get; set; } = null!;
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
