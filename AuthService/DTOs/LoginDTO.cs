using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService;

public class LoginDTO
{
    [Required]
    public string? Email {get; set;}

    [Required]
    [MinLength(6)]
    public string? Password {get;set;}
}
