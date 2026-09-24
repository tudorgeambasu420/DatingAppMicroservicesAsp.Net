using System;
using System.Text.Json;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.ServiceContracts;
using Confluent.Kafka;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services;

public class AuthServices : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthServices(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            throw new Exception("Invalid username or password");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, false);
        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return await _tokenService.GenerateAuthResponseAsync(user);
        }
        else
        {
            throw new Exception("Invalid username or password");
        }
    }

    public async Task<bool> LogoutAllDevicesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new Exception("Invalid id");
        return await _tokenService.RevokeRefreshTokenAsync(userId);
    }

    public async Task<bool> LogoutAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("Invalid id");
        }
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("Invalid token");
        }
        return await _tokenService.LogoutAsync(userId, token);
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO request, CancellationToken cancellationToken)
    {
        var testUser = await _userManager.FindByEmailAsync(request.Email!);
        if (testUser != null)
        {
            throw new Exception("The email is already in use");
        }
        var user = new User
        {
            Email = request.Email,
            UserName = request.Username,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };



        var result = await _userManager.CreateAsync(user, request.Password!);

        if (result.Succeeded)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                Acks = Acks.All,
                EnableIdempotence = true,
                AllowAutoCreateTopics = true

            };
            using var producer = new ProducerBuilder<string, string>(config).Build();

            UserCreatedEventDTO userDto = new UserCreatedEventDTO
            {
                UserId = user.Id,
                UserName = user.UserName!
            };

            var message = new Message<string, string>
            {
                Key = user.Id,
                Value = JsonSerializer.Serialize(userDto)

            };

            await producer.ProduceAsync("UserCreatedEvent", message, cancellationToken);

            return await _tokenService.GenerateAuthResponseAsync(user);
            
        }
        else
        {
            throw new Exception("Something went wrong");
        }

    }
}
