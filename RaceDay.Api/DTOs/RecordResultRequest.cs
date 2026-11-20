
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class RecordResultRequest
{
    [Range(1, int.MaxValue)]
    public int EnrolmentId { get; set; }

    [Required]
    public TimeSpan? FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int Position { get; set; }
}
