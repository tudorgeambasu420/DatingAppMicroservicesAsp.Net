using System;
using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Entities;

namespace AuthService.ServiceContracts;

public interface ITokenService
{
    Task<AuthResponseDTO> GenerateAuthResponseAsync(User user);
    Task<string> GenerateJwtTokenAsync(User user);
    string GenerateRefreshToken();
    Task<TokenValidationResponseDTO> ValidateTokenAsync(string token);
    Task<User?> GetUserFromTokenAsync(string token);
    Task<string?> GetUserIdFromTokenAsync(string token);
    Task<string?> GetEmailFromTokenAsync(string token);
    Task<List<string>> GetRolesFromTokenAsync(string token);
    Task<bool> IsTokenExpiredAsync(string token);
    Task<DateTime?> GetTokenExpirationAsync(string token);
    Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenDTO dto);
    Task<bool> RevokeRefreshTokenAsync(string userId);
    Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    Task<bool> LogoutAsync(string userId, string token);
}
