using System;

namespace MemberService.Entities;

public class Member
{
    
    public int Id{get;set;}

    public required string UserId {get;set;}

    public required string UserName {get;set;}

    public string? Description {get;set;}

    public string? Location {get;set;}
}
