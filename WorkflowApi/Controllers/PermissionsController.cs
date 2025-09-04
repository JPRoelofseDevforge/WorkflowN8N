using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowApi.Services;

namespace WorkflowApi.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(Policy = "AdminOnly")]
public class PermissionsController : ControllerBase
{
    private readonly PermissionManagementService _permissionService;

    public PermissionsController(PermissionManagementService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        try
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        try
        {
            var permission = await _permissionService.CreatePermissionAsync(
                request.Name, request.Description, request.Resource, request.Action);
            return CreatedAtAction(nameof(GetAllPermissions), new { id = permission.Id }, permission);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionRequest request)
    {
        try
        {
            var permission = await _permissionService.UpdatePermissionAsync(
                id, request.Name, request.Description, request.Resource, request.Action);
            return Ok(permission);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        try
        {
            await _permissionService.DeletePermissionAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{permissionId}/roles/{roleId}")]
    public async Task<IActionResult> AssignPermissionToRole(int permissionId, int roleId)
    {
        try
        {
            await _permissionService.AssignPermissionToRoleAsync(permissionId, roleId);
            return Ok(new { message = "Permission assigned to role successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{permissionId}/roles/{roleId}")]
    public async Task<IActionResult> RemovePermissionFromRole(int permissionId, int roleId)
    {
        try
        {
            await _permissionService.RemovePermissionFromRoleAsync(permissionId, roleId);
            return Ok(new { message = "Permission removed from role successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class UpdatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}