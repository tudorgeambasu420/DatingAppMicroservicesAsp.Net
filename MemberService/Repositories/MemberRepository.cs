using System;
using MemberService.Data;
using MemberService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Repositories;

public class MemberRepository
{
    private readonly AppDbContext _context;

    public MemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Member>?> GetMembersFromLocation(string userId)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m=>m.UserId == userId);
        if(member is null)
            return null;
        var list = await _context.Members.Where(m => m.UserId != member.UserId 
                && member.Location == m.Location)
                .OrderBy(m => m.UserName).Take(5).ToListAsync();
        return list;
    }
    public async Task<Member?> GetMe(string userId)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m=> m.UserId == userId);
        if(member is null)
            return null;
        else
            return member;
    }

    public async Task<Member?> Create(Member member)
    {
        _context.Members.Add(member);
        var result = await _context.SaveChangesAsync();
        if(result > 0)
        {
            return member;
        }
        else
        {
           return null;
        }
        
    }

    public async Task<Member?> Update(Member memberUpdated)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == memberUpdated.UserId);
        if(member == null)
        {
            return null;
        }
        member.Description = memberUpdated.Description;
        member.Location = memberUpdated.Location;
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return member;
        }
        else
            return null;
    }

    public async Task<Member?> Delete(string userId)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if(member == null)
            return null;
        _context.Members.Remove(member);
        var result = await _context.SaveChangesAsync();
        if(result > 0)
        {
            return member;
        }
        else
            return null;
        
    }
}
