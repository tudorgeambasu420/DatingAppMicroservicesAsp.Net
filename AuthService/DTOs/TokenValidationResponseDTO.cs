using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class TokenValidationResponseDTO
{
    [Required]
    public bool IsValid { get; set; }
    [Required]
    public string? UserId { get; set; }
    [Required]
    public string? Email { get; set; }
    public List<string>? Roles { get; set; }
}
