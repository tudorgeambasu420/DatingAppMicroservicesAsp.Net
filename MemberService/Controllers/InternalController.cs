using System.Security.Claims;
using MemberService.Entities;
using MemberService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MemberService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController: ControllerBase
    {
        /*
        [HttpGet("me")]
        public async Task<ActionResult<Member>> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        */
    }
}
