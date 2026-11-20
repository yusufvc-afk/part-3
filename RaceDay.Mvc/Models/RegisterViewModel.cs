using System.ComponentModel.DataAnnotations;
public sealed class RegisterViewModel
{
    [Required, StringLength(50)] public string FirstName { get; set; } = "";
    [Required, StringLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required] public string Role { get; set; } = "Participant";
    [Phone] public string? PhoneNumber { get; set; }
}
