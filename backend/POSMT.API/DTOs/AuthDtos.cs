using System.ComponentModel.DataAnnotations;

namespace POSMT.API.DTOs;

public class LoginRequest
{
    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponse(string Token, DateTime ExpiresAtUtc, int UserId, string Name, string Email, string Role);

public record CurrentUserDto(int UserId, string Name, string Email, string Role);
