using System;
using MemberService.DTOs;
using MemberService.Entities;
using Microsoft.AspNetCore.Identity;

namespace MemberService.ExtensionMethods;

public static class MemberExtensions
{
    public static Member ToMemberEntity(this UserCreatedEventDTO user)
    {
        var member = new Member
        {
            UserId = user.UserId,
            UserName = user.UserName
        };
        return member;
    }
}
