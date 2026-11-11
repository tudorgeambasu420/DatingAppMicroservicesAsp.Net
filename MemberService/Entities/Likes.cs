using System;

namespace MemberService.Entities;

public class Likes
{
    public int Id{get;set;}
    public int LikerMemberId {get;set;}
    public int LikedMemberId{get;set;}
}
