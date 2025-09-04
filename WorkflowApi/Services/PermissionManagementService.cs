using Microsoft.EntityFrameworkCore;
using WorkflowApi.Data;
using WorkflowApi.Models;

namespace WorkflowApi.Services;

public class PermissionManagementService
{
    private readonly WorkflowContext _context;

    public PermissionManagementService(WorkflowContext context)
    {
        _context = context;
    }

    public async Task<Permission> CreatePermissionAsync(string name, string description, string resource, string action)
    {
        if (await _context.Permissions.AnyAsync(p => p.Name == name))
            throw new InvalidOperationException("Permission with this name already exists.");

        var permission = new Permission
        {
            Name = name,
            Description = description,
            Resource = resource,
            Action = action
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task<Permission> UpdatePermissionAsync(int id, string name, string description, string resource, string action)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            throw new KeyNotFoundException("Permission not found.");

        if (await _context.Permissions.AnyAsync(p => p.Name == name && p.Id != id))
            throw new InvalidOperationException("Permission with this name already exists.");

        permission.Name = name;
        permission.Description = description;
        permission.Resource = resource;
        permission.Action = action;

        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task DeletePermissionAsync(int id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            throw new KeyNotFoundException("Permission not found.");

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();
    }

    public async Task AssignPermissionToRoleAsync(int permissionId, int roleId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        if (permission == null)
            throw new KeyNotFoundException("Permission not found.");

        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException("Role not found.");

        if (await _context.RolePermissions.AnyAsync(rp => rp.PermissionId == permissionId && rp.RoleId == roleId))
            throw new InvalidOperationException("Permission is already assigned to this role.");

        var rolePermission = new RolePermission
        {
            PermissionId = permissionId,
            RoleId = roleId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
    }

    public async Task RemovePermissionFromRoleAsync(int permissionId, int roleId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.PermissionId == permissionId && rp.RoleId == roleId);

        if (rolePermission == null)
            throw new KeyNotFoundException("Permission is not assigned to this role.");

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions.ToListAsync();
    }

    public async Task<List<Permission>> GetPermissionsForRoleAsync(int roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException("Role not found.");

        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }
}