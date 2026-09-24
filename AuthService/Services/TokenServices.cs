using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.ServiceContracts;
using AuthService.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class TokenServices : ITokenService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly IMemoryCache _cache;

    public TokenServices(
        UserManager<User> userManager,
        IOptions<JwtSettings> jwtSettings,
        IMemoryCache cache)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _cache = cache;
    }
    public async Task<bool> LogoutAsync(string userId, string token)
    {
        _cache.Set($"blacklist_{token}", true, TimeSpan.FromMinutes(15));

        // Șterge refresh token-ul
        await RevokeRefreshTokenAsync(userId);

        return true;
    }

    public async Task<AuthResponseDTO> GenerateAuthResponseAsync(User user)
    {
        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDTO
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes)
        };
    }

    public async Task<string> GenerateJwtTokenAsync(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<TokenValidationResponseDTO> ValidateTokenAsync(string token)
    {
        Console.WriteLine($"=== VALIDATING TOKEN ===");
        Console.WriteLine($"Token: {token?.Substring(0, Math.Min(30, token?.Length ?? 0))}...");
        if (_cache.TryGetValue($"blacklist_{token}", out _))
        {
            return new TokenValidationResponseDTO
            {
                IsValid = false,
                Roles = new List<string>()
            };
        }
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();


            return new TokenValidationResponseDTO
            {
                IsValid = true,
                UserId = userId,
                Email = email,
                Roles = roles
            };
        }
        catch
        {
            return new TokenValidationResponseDTO
            {
                IsValid = false,
                Roles = new List<string>()
            };
        }
    }

    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        if (_cache.TryGetValue($"blacklist_{token}", out _))
            return null;
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetUserIdFromTokenAsync(string token)
    {
        if (_cache.TryGetValue($"blacklist_{token}", out _))
            return null;
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetEmailFromTokenAsync(string token)
    {
        if (_cache.TryGetValue($"blacklist_{token}", out _))
            return null;
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<string>> GetRolesFromTokenAsync(string token)
    {
        if (_cache.TryGetValue($"blacklist_{token}", out _))
            return new List<string>();
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            return principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<bool> IsTokenExpiredAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    public async Task<DateTime?> GetTokenExpirationAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenDTO dto)
    {
        if (_cache.TryGetValue($"blacklist_{dto.Token}", out _))
            throw new UnauthorizedAccessException("Token is blacklisted");
        var principal = GetPrincipalFromExpiredToken(dto.Token!);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Invalid token");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        if (user.RefreshToken != dto.RefreshToken)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        return user.RefreshToken == refreshToken &&
               user.RefreshTokenExpiryTime > DateTime.UtcNow;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        return principal;
    }
}
