using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WorkflowApi.Data;
using WorkflowApi.Models;

namespace WorkflowApi.Services;

public class AuthService
{
    private readonly WorkflowContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public AuthService(WorkflowContext context, JwtService jwtService, PasswordService passwordService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterUserAsync(RegisterRequestDto request)
    {
        // Check if username or email already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new Exception("Username already exists");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email already exists");

        // Hash password
        var hashedPassword = _passwordService.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hashedPassword,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assign default role (create if doesn't exist)
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (defaultRole == null)
        {
            defaultRole = new Role
            {
                Name = "User",
                Description = "Default user role"
            };
            _context.Roles.Add(defaultRole);
            await _context.SaveChangesAsync();
        }
        user.Roles.Add(defaultRole);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginUserAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new Exception("Invalid username or password");

        if (!user.IsActive)
            throw new Exception("Account is deactivated");

        // Generate tokens
        return await GenerateAuthResponseAsync(user);
    }

    public async Task LogoutUserAsync(int userId)
    {
        // Revoke all refresh tokens for the user
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Invalid or expired refresh token");

        // Revoke the old refresh token
        tokenEntity.IsRevoked = true;

        // Generate new tokens
        var user = tokenEntity.User;
        return await GenerateAuthResponseAsync(user);
    }

    public async Task<bool> ValidateUserCredentials(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        return user != null && _passwordService.VerifyPassword(password, user.PasswordHash) && user.IsActive;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var permissions = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.Permissions)
            .Select(p => p.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
        // Create claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        // Add roles to claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        // Generate access token
        var accessToken = _jwtService.GenerateAccessToken(claims);

        // Generate refresh token
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Save refresh token to database
        var jwtSettings = _configuration.GetSection("Jwt");
        var refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        // Get user permissions
        var permissions = await GetUserPermissionsAsync(user.Id);

        // Create user DTO
        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Permissions = permissions
        };

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userDto
        };
    }
}