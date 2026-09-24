using System;
using MemberService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Data;

public class AppDbContext : DbContext
{
    public DbSet<Member> Members {get; set;} = null!;
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Member>()
            .HasIndex(m => m.UserId)
            .IsUnique();
    }
}
