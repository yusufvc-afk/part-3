
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }
}
