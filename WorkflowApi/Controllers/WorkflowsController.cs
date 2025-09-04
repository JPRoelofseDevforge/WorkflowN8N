using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowApi.Attributes;
using WorkflowApi.Data;
using WorkflowApi.Models;
using WorkflowApi.Services;

namespace WorkflowApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly WorkflowContext _context;
    private readonly N8nService _n8nService;
    private readonly WorkflowAccessService _workflowAccessService;
    private readonly AuthorizationService _authorizationService;
    private readonly WorkflowStepService _stepService;

    public WorkflowsController(WorkflowContext context, N8nService n8nService, WorkflowAccessService workflowAccessService, AuthorizationService authorizationService, WorkflowStepService stepService)
    {
        _context = context;
        _n8nService = n8nService;
        _workflowAccessService = workflowAccessService;
        _authorizationService = authorizationService;
        _stepService = stepService;
    }

    // GET: api/workflows
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetWorkflows()
    {
        var workflows = await _workflowAccessService.FilterWorkflowsByPermissionsAsync();

        // If database is empty, sync from n8n
        if (!workflows.Any())
        {
            try
            {
                var n8nWorkflows = await _n8nService.GetWorkflowsAsync();
                foreach (var n8nWorkflow in n8nWorkflows)
                {
                    var workflow = new Workflow
                    {
                        Name = n8nWorkflow.Name,
                        Description = $"Synced from n8n: {n8nWorkflow.Name}",
                        IsActive = n8nWorkflow.Active,
                        N8nId = n8nWorkflow.Id,
                        CreatedAt = DateTime.Parse(n8nWorkflow.CreatedAt)
                    };
                    _context.Workflows.Add(workflow);
                }
                await _context.SaveChangesAsync();
                workflows = await _workflowAccessService.FilterWorkflowsByPermissionsAsync();
            }
            catch (Exception ex)
            {
                // Log error but continue with empty list if sync fails
                Console.WriteLine($"Error syncing workflows from n8n: {ex.Message}");
            }
        }

        var dtos = workflows.Select(w => new WorkflowDto
        {
            Id = w.Id,
            N8nId = w.N8nId,
            Name = w.Name,
            Description = w.Description,
            CreatedAt = w.CreatedAt,
            IsActive = w.IsActive,
            ViewPermissionId = w.ViewPermissionId,
            ViewPermissionName = w.ViewPermission?.Name,
            EditPermissionId = w.EditPermissionId,
            EditPermissionName = w.EditPermission?.Name,
            ExecutePermissionId = w.ExecutePermissionId,
            ExecutePermissionName = w.ExecutePermission?.Name,
            ManagePermissionId = w.ManagePermissionId,
            ManagePermissionName = w.ManagePermission?.Name,
            Steps = w.WorkflowSteps.Select(ws => new WorkflowStepDto
            {
                Id = ws.Id,
                StepName = ws.StepName,
                StepType = ws.StepType,
                IsRequired = ws.IsRequired,
                Description = ws.Description,
                Order = ws.Order,
                RequiredPermissionId = ws.RequiredPermissionId,
                RequiredPermissionName = ws.RequiredPermission?.Name
            }).ToList()
        });
        return Ok(dtos);
    }

    // POST: api/workflows
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WorkflowDto>> CreateWorkflow(CreateWorkflowDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Check if user has permission to create workflows
        var userIdForLogging = _authorizationService.GetCurrentUserId();
        Console.WriteLine($"[DEBUG] CreateWorkflow: Attempting to create workflow for user {userIdForLogging}");

        var hasPermission = await _authorizationService.HasPermissionAsync("CanCreateWorkflow");
        Console.WriteLine($"[DEBUG] CreateWorkflow: User {userIdForLogging} has 'CanCreateWorkflow' permission: {hasPermission}");

        if (!hasPermission)
        {
            Console.WriteLine($"[DEBUG] CreateWorkflow: Forbidding workflow creation for user {userIdForLogging} - missing 'CanCreateWorkflow' permission");
            return Forbid();
        }

        try
        {
            // Create workflow in n8n first
            var n8nWorkflow = await _n8nService.CreateWorkflowAsync(new CreateN8nWorkflowDto
            {
                Name = dto.Name,
                Nodes = new List<object>(), // Empty nodes for basic workflow
                Connections = new Dictionary<string, object>(), // Empty connections
                Settings = new Dictionary<string, object> { ["executionOrder"] = "v1" } // Settings with executionOrder
            });

            // Save to database with n8n ID
            var currentUserId = _authorizationService.GetCurrentUserId();
            var workflow = new Workflow
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                N8nId = n8nWorkflow.Id,
                CreatedById = currentUserId,
                ViewPermissionId = dto.ViewPermissionId,
                EditPermissionId = dto.EditPermissionId,
                ExecutePermissionId = dto.ExecutePermissionId,
                ManagePermissionId = dto.ManagePermissionId
            };

            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync();

            // Create default permissions if not provided
            if (dto.ViewPermissionId == null)
            {
                var viewPermission = new Permission
                {
                    Name = $"ViewWorkflow{workflow.Id}",
                    Description = $"Permission to view workflow {workflow.Name}",
                    Resource = "Workflow",
                    Action = "View"
                };
                _context.Permissions.Add(viewPermission);
                await _context.SaveChangesAsync();
                workflow.ViewPermissionId = viewPermission.Id;
            }

            if (dto.EditPermissionId == null)
            {
                var editPermission = new Permission
                {
                    Name = $"EditWorkflow{workflow.Id}",
                    Description = $"Permission to edit workflow {workflow.Name}",
                    Resource = "Workflow",
                    Action = "Edit"
                };
                _context.Permissions.Add(editPermission);
                await _context.SaveChangesAsync();
                workflow.EditPermissionId = editPermission.Id;
            }

            if (dto.ExecutePermissionId == null)
            {
                var executePermission = new Permission
                {
                    Name = $"ExecuteWorkflow{workflow.Id}",
                    Description = $"Permission to execute workflow {workflow.Name}",
                    Resource = "Workflow",
                    Action = "Execute"
                };
                _context.Permissions.Add(executePermission);
                await _context.SaveChangesAsync();
                workflow.ExecutePermissionId = executePermission.Id;
            }

            if (dto.ManagePermissionId == null)
            {
                var managePermission = new Permission
                {
                    Name = $"ManageWorkflow{workflow.Id}",
                    Description = $"Permission to manage workflow {workflow.Name}",
                    Resource = "Workflow",
                    Action = "Manage"
                };
                _context.Permissions.Add(managePermission);
                await _context.SaveChangesAsync();
                workflow.ManagePermissionId = managePermission.Id;

                // Assign manage permission to creator's roles
                if (currentUserId.HasValue)
                {
                    var userRoles = await _context.UserRoles
                        .Where(ur => ur.UserId == currentUserId.Value)
                        .Include(ur => ur.Role)
                        .ToListAsync();

                    foreach (var userRole in userRoles)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = userRole.RoleId,
                            PermissionId = managePermission.Id
                        };
                        _context.RolePermissions.Add(rolePermission);
                    }
                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();

            var responseDto = new WorkflowDto
            {
                Id = workflow.Id,
                N8nId = workflow.N8nId,
                Name = workflow.Name,
                Description = workflow.Description,
                CreatedAt = workflow.CreatedAt,
                IsActive = workflow.IsActive,
                ViewPermissionId = workflow.ViewPermissionId,
                EditPermissionId = workflow.EditPermissionId,
                ExecutePermissionId = workflow.ExecutePermissionId,
                ManagePermissionId = workflow.ManagePermissionId
            };

            return CreatedAtAction(nameof(GetWorkflows), new { id = workflow.Id }, responseDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating workflow: {ex.Message}");
        }
    }

    // PUT: api/workflows/{n8nId}
    [HttpPut("{n8nId}")]
    [Authorize]
    public async Task<IActionResult> UpdateWorkflow(string n8nId, UpdateWorkflowDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.N8nId == n8nId);
        if (workflow == null) return NotFound();

        // Check if user can edit this workflow
        if (!await _workflowAccessService.CanEditWorkflowAsync(workflow.Id))
            return Forbid();

        try
        {
            // Update in n8n if it has an n8n ID
            if (!string.IsNullOrEmpty(workflow.N8nId))
            {
                await _n8nService.UpdateWorkflowAsync(workflow.N8nId, new UpdateN8nWorkflowDto
                {
                    Name = dto.Name,
                    Nodes = null, // Keep existing nodes
                    Connections = null // Keep existing connections
                });
            }

            // Update in database
            workflow.Name = dto.Name;
            workflow.Description = dto.Description;
            workflow.IsActive = dto.IsActive;
            workflow.ViewPermissionId = dto.ViewPermissionId;
            workflow.EditPermissionId = dto.EditPermissionId;
            workflow.ExecutePermissionId = dto.ExecutePermissionId;
            workflow.ManagePermissionId = dto.ManagePermissionId;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating workflow: {ex.Message}");
        }
    }

    // DELETE: api/workflows/{n8nId}
    [HttpDelete("{n8nId}")]
    [Authorize]
    public async Task<IActionResult> DeleteWorkflow(string n8nId)
    {
        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.N8nId == n8nId);
        if (workflow == null) return NotFound();

        // Check if user can manage this workflow
        if (!await _workflowAccessService.CanManageWorkflowAsync(workflow.Id))
            return Forbid();

        try
        {
            // Delete from n8n if it has an n8n ID
            if (!string.IsNullOrEmpty(workflow.N8nId))
            {
                try
                {
                    await _n8nService.DeleteWorkflowAsync(workflow.N8nId);
                }
                catch (Exception ex)
                {
                    // Log but continue - workflow might not exist in n8n
                    Console.WriteLine($"Warning: Could not delete workflow from n8n: {ex.Message}");
                }
            }

            // Delete from database
            _context.Workflows.Remove(workflow);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting workflow: {ex.Message}");
        }
    }

    // PUT: api/workflows/{n8nId}/toggle
    [HttpPut("{n8nId}/toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleWorkflow(string n8nId)
    {
        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.N8nId == n8nId);
        if (workflow == null) return NotFound();

        // Check if user can manage this workflow
        if (!await _workflowAccessService.CanManageWorkflowAsync(workflow.Id))
            return Forbid();

        try
        {
            // Toggle in n8n if it has an n8n ID
            if (!string.IsNullOrEmpty(workflow.N8nId))
            {
                await _n8nService.ToggleWorkflowAsync(workflow.N8nId);
            }

            // Update database
            workflow.IsActive = !workflow.IsActive;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error toggling workflow: {ex.Message}");
        }
    }

    // POST: api/workflows/{n8nId}/execute
    [HttpPost("{n8nId}/execute")]
    [Authorize]
    public async Task<IActionResult> ExecuteWorkflow(string n8nId)
    {
        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.N8nId == n8nId);
        if (workflow == null) return NotFound();

        // Check if user can execute this workflow
        if (!await _workflowAccessService.CanExecuteWorkflowAsync(workflow.Id))
            return Forbid();

        if (!workflow.IsActive) return BadRequest("Workflow is not active");

        // Validate step permissions before execution
        var steps = await _context.WorkflowSteps
            .Where(ws => ws.WorkflowId == workflow.Id)
            .Include(ws => ws.WorkflowStepPermissions)
            .ThenInclude(wsp => wsp.Permission)
            .ToListAsync();

        var permissionViolations = new List<string>();
        foreach (var step in steps)
        {
            if (step.IsRequired)
            {
                var hasExecutePermission = await _stepService.ValidateStepPermissionsAsync(step.Id, "execute");
                if (!hasExecutePermission)
                {
                    permissionViolations.Add($"No execute permission for required step: {step.StepName}");
                }
            }
        }

        if (permissionViolations.Any())
        {
            // Log permission violations
            foreach (var violation in permissionViolations)
            {
                Console.WriteLine($"Permission violation during workflow execution: {violation}");
            }
            return Forbid("Insufficient permissions to execute workflow steps");
        }

        try
        {
            // Execute in n8n if it has an n8n ID
            N8nExecutionDto? n8nExecution = null;
            if (!string.IsNullOrEmpty(workflow.N8nId))
            {
                n8nExecution = await _n8nService.ExecuteWorkflowAsync(workflow.N8nId);
            }

            // Log execution in database
            var execution = new Execution
            {
                WorkflowId = workflow.Id,
                Status = n8nExecution?.Status ?? "Running",
                StartedAt = n8nExecution?.StartedAt ?? DateTime.UtcNow
            };

            _context.Executions.Add(execution);
            await _context.SaveChangesAsync();

            // If n8n execution completed immediately, update status
            if (n8nExecution != null && n8nExecution.FinishedAt.HasValue)
            {
                execution.CompletedAt = n8nExecution.FinishedAt;
                execution.Status = n8nExecution.Status;
                await _context.SaveChangesAsync();
            }

            return Ok(new { executionId = execution.Id, n8nExecutionId = n8nExecution?.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error executing workflow: {ex.Message}");
        }
    }
}