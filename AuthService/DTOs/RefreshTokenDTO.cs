using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService;

public class RefreshTokenDTO
{
    [Required]
    public string? Token { get; set; }
    
    [Required]
    public string? RefreshToken { get; set; }
}
