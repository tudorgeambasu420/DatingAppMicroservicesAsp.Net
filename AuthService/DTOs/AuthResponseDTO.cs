using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class AuthResponseDTO
{
    [Required]
    public string? Token { get; set; }
    [Required]
    public string? RefreshToken { get; set; }
    [Required]
    public string? UserId { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [MinLength(3)]
    public string? Username { get; set; }
    [Required]
    public DateTime ExpiresAt { get; set; }
}
