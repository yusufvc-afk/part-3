
namespace RaceDay.Api.Models;

public class EventCategory
{
    public int EventCategoryId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public decimal EntryFee { get; set; }
    public int MaximumParticipants { get; set; }

    public RaceEvent Event { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
