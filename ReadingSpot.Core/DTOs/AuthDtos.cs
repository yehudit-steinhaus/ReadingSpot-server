using System.ComponentModel.DataAnnotations;

namespace ReadingSpot.Core.DTOs;

public class RegisterDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = "";
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}
