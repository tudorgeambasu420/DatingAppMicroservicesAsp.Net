using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class UserCreatedEventDTO
{
    [Required]
    public required string UserId {get;set;}

    [Required]
    public required string UserName {get;set;}
}
