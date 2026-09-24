using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class RegisterDTO
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [MinLength(6)]
    public string? Password { get; set; }
    [Required]
    [MinLength(6)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword {get;set;}
    [Required]
    [MinLength(3)]
    public string? Username { get; set; }

}
