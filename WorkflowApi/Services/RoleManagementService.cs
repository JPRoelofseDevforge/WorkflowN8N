using Microsoft.EntityFrameworkCore;
using WorkflowApi.Data;
using WorkflowApi.Models;

namespace WorkflowApi.Services;

public class RoleManagementService
{
    private readonly WorkflowContext _context;

    public RoleManagementService(WorkflowContext context)
    {
        _context = context;
    }

    public async Task<Role> CreateRoleAsync(string name, string description)
    {
        if (await _context.Roles.AnyAsync(r => r.Name == name))
            throw new InvalidOperationException("Role with this name already exists.");

        var role = new Role
        {
            Name = name,
            Description = description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateRoleAsync(int id, string name, string description)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            throw new KeyNotFoundException("Role not found.");

        if (await _context.Roles.AnyAsync(r => r.Name == name && r.Id != id))
            throw new InvalidOperationException("Role with this name already exists.");

        role.Name = name;
        role.Description = description;

        await _context.SaveChangesAsync();
        return role;
    }

    public async Task DeleteRoleAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            throw new KeyNotFoundException("Role not found.");

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

    public async Task AssignRoleToUserAsync(int roleId, int userId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException("Role not found.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId && ur.UserId == userId))
            throw new InvalidOperationException("Role is already assigned to this user.");

        var userRole = new UserRole
        {
            RoleId = roleId,
            UserId = userId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRoleFromUserAsync(int roleId, int userId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.RoleId == roleId && ur.UserId == userId);

        if (userRole == null)
            throw new KeyNotFoundException("Role is not assigned to this user.");

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<List<Role>> GetRolesForUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync();
    }
}