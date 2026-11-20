
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class AssignCategoryRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, 99999999.99)]
    public decimal EntryFee { get; set; }

    [Range(1, int.MaxValue)]
    public int MaximumParticipants { get; set; }
}
