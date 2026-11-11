using System.Security.Claims;
using MemberService.DTOs;
using MemberService.Entities;
using MemberService.Repositories;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MemberService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController: ControllerBase
    {

        private readonly MemberRepository _memberRepo;
        private readonly IConfiguration _configuration;

        public MembersController(MemberRepository memberRepository, IConfiguration configuration)
        {
            _memberRepo = memberRepository;
            _configuration = configuration;
        }
        [HttpGet("@me")]
        [Authorize]
        public async Task<ActionResult<Member>> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId is null)
            {
                return BadRequest("The userId must not be null");
            }
            return Ok(await _memberRepo.GetMe(userId));
        }

        [HttpGet("feed")]
        [Authorize]

        public async Task<ActionResult<List<Member>?>> GetFeed()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId is null)
            {
                return BadRequest("The userId must not be null");
            }
            return Ok(await _memberRepo.GetMembersFromLocation(userId));
        }
        
        [HttpDelete("@me")]
        [Authorize]
        public async Task<ActionResult> DeleteMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId is null)
            {
                return BadRequest("The userId must not be null");
            }
            
        
            var token = Request.Headers.Authorization.ToString();
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_configuration["AuthServiceUrl"]}/api/auth/users/{userId}");
            request.Headers.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(token);
            using var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Something went wrong with auth");
            }
            else
            {
                var result = await _memberRepo.Delete(userId);
                if(result is null)
                {
                    return BadRequest("Something went wrong");
                }
                return NoContent();
            }
                

        }
        [HttpPut("@me")]
        [Authorize]

        public async Task<ActionResult> UpdateMe([FromBody] UpdateMemberDTO request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId is null)
            {
                return Unauthorized("Something Went Wrong");
            }
            var member = await _memberRepo.GetMe(userId);
            if(member is null)
            {
                return NotFound("Something Went Wrong");
            }
            member.Description = request.Description;
            member.Location = request.Location;

            var result = await _memberRepo.Update(member);
            if(result is null)
            {
                return BadRequest("Something Went Wrong");
            }
            return NoContent();
        }
        
    }
}
