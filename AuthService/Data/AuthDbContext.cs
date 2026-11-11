using System;
using AuthService.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;


public class AuthDbContext : IdentityDbContext<User>
{
    
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        
    }

}
