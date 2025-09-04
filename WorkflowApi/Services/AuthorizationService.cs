using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkflowApi.Data;
using WorkflowApi.Models;

namespace WorkflowApi.Services;

public class AuthorizationService
{
    private readonly WorkflowContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationService(WorkflowContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> HasRoleAsync(string roleName)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return false;

        return await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
    }

    public async Task<bool> HasPermissionAsync(string permissionName)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            Console.WriteLine($"[DEBUG] HasPermissionAsync: User not authenticated for permission '{permissionName}'");
            return false;
        }

        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        Console.WriteLine($"[DEBUG] HasPermissionAsync: User {userId} has roles: {string.Join(", ", userRoles.Select(ur => ur.Role.Name))}");

        foreach (var userRole in userRoles)
        {
            var rolePermissions = userRole.Role.Permissions.Select(p => p.Name).ToList();
            Console.WriteLine($"[DEBUG] HasPermissionAsync: Role '{userRole.Role.Name}' has permissions: {string.Join(", ", rolePermissions)}");

            if (rolePermissions.Contains(permissionName))
            {
                Console.WriteLine($"[DEBUG] HasPermissionAsync: Permission '{permissionName}' found for user {userId}");
                return true;
            }
        }

        Console.WriteLine($"[DEBUG] HasPermissionAsync: Permission '{permissionName}' NOT found for user {userId}");
        return false;
    }

    public async Task<List<string>> GetUserRolesAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return new List<string>();

        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }

    public async Task<List<string>> GetUserPermissionsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return new List<string>();

        return await _context.UserRoles
            .Include(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.Permissions.Select(p => p.Name))
            .Distinct()
            .ToListAsync();
    }

    public int? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;

        return userId;
    }
}