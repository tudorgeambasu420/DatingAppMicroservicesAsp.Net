using System.Security.Claims;
using AuthService.DTOs;
using AuthService.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterDTO registerRequest, CancellationToken cancellationToken)
        {
            return Ok(await _authService.RegisterAsync(registerRequest, cancellationToken));
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO loginRequest)
        {
            return Ok(await _authService.LoginAsync(loginRequest));
        }
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDTO>> Refresh([FromBody] RefreshTokenDTO request)
        {
            try
            {
                return Ok(await _tokenService.RefreshTokenAsync(request));
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityTokenException or ArgumentException)
            {
                return Unauthorized(new { message = "Invalid or expired refresh credentials." });
            }
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<bool>> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required");

            var result = await _authService.LogoutAsync(userId, token);
            if (!result)
                return BadRequest("Logout failed");

            return Ok("Logged out successfully");
        }

    }
}
