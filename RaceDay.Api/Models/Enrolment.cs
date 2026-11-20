
namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public int EventCategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Registered";

    public User Participant { get; set; } = null!;
    public EventCategory EventCategory { get; set; } = null!;
    public RaceResult? Result { get; set; }
}
