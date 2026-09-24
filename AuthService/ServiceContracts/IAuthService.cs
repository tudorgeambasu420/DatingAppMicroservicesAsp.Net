using System;
using AuthService.DTOs;

namespace AuthService;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(RegisterDTO request, CancellationToken cancellationToken);
    Task<AuthResponseDTO> LoginAsync(LoginDTO request);
    Task<bool> LogoutAsync(string userId, string token);
    Task<bool> LogoutAllDevicesAsync(string userId);

}
