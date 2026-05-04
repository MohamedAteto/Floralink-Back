using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FloraLink.Application.DTOs.Auth;
using FloraLink.Application.Interfaces;
using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FloraLink.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        try
        {
            if (await _users.ExistsAsync(dto.Email))
                throw new InvalidOperationException("Email already registered.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            var created = await _users.AddAsync(user);
            return BuildResponse(created);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            throw new InvalidOperationException("Registration failed. Please try again.");
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Hardcoded demo user — look up real ID from DB if exists
        if (dto.Email == "demo@floralink.io" && dto.Password == "demo1234")
        {
            var dbUser = await _users.GetByEmailAsync(dto.Email);
            if (dbUser != null) return BuildResponse(dbUser);
            // fallback if DB user doesn't exist yet
            var demoUser = new User { Id = 1, Username = "demo", Email = "demo@floralink.io", PasswordHash = "" };
            return BuildResponse(demoUser);
        }

        var user = await _users.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");
        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user)
    {
        var token = GenerateJwt(user);
        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            UserId = user.Id
        };
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
