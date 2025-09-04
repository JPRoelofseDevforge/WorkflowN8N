using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowApi.Data;
using WorkflowApi.Models;
using WorkflowApi.Services;

namespace WorkflowApi.Controllers;

[ApiController]
[Route("api/workflows/{workflowId}/steps")]
[Authorize]
public class WorkflowStepController : ControllerBase
{
    private readonly WorkflowStepService _stepService;
    private readonly WorkflowContext _context;

    public WorkflowStepController(WorkflowStepService stepService, WorkflowContext context)
    {
        _stepService = stepService;
        _context = context;
    }

    // GET: api/workflows/{workflowId}/steps
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowStepDto>>> GetSteps(int workflowId)
    {
        try
        {
            var steps = await _stepService.GetStepsForWorkflowAsync(workflowId);
            var dtos = steps.Select(s => new WorkflowStepDto
            {
                Id = s.Id,
                StepName = s.StepName,
                StepType = s.StepType,
                IsRequired = s.IsRequired,
                Description = s.Description,
                Order = s.Order,
                RequiredPermissionId = s.RequiredPermissionId,
                RequiredPermissionName = s.RequiredPermission?.Name
            });
            return Ok(dtos);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST: api/workflows/{workflowId}/steps
    [HttpPost]
    public async Task<ActionResult<WorkflowStepDto>> CreateStep(int workflowId, CreateWorkflowStepDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var step = await _stepService.AddStepAsync(workflowId, dto.StepName, dto.StepType, dto.IsRequired, dto.Description, dto.Order);
            var responseDto = new WorkflowStepDto
            {
                Id = step.Id,
                StepName = step.StepName,
                StepType = step.StepType,
                IsRequired = step.IsRequired,
                Description = step.Description,
                Order = step.Order,
                RequiredPermissionId = step.RequiredPermissionId,
                RequiredPermissionName = step.RequiredPermission?.Name
            };
            return CreatedAtAction(nameof(GetSteps), new { workflowId }, responseDto);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/workflows/{workflowId}/steps/{stepId}
    [HttpPut("{stepId}")]
    public async Task<IActionResult> UpdateStep(int workflowId, int stepId, UpdateWorkflowStepDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _stepService.UpdateStepAsync(stepId, dto.StepName, dto.StepType, dto.IsRequired, dto.Description, dto.Order);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/workflows/{workflowId}/steps/{stepId}
    [HttpDelete("{stepId}")]
    public async Task<IActionResult> DeleteStep(int workflowId, int stepId)
    {
        try
        {
            var result = await _stepService.RemoveStepAsync(stepId);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // GET: api/workflows/{workflowId}/steps/{stepId}/permissions
    [HttpGet("{stepId}/permissions")]
    public async Task<ActionResult<IEnumerable<WorkflowStepPermissionDto>>> GetStepPermissions(int workflowId, int stepId)
    {
        var step = await _context.WorkflowSteps
            .Include(s => s.WorkflowStepPermissions)
            .ThenInclude(wsp => wsp.Permission)
            .FirstOrDefaultAsync(s => s.Id == stepId && s.WorkflowId == workflowId);

        if (step == null) return NotFound();

        // Check if user can manage the workflow
        var canManage = await _stepService.ValidateStepPermissionsAsync(stepId, "modify");
        if (!canManage) return Forbid();

        var dtos = step.WorkflowStepPermissions.Select(wsp => new WorkflowStepPermissionDto
        {
            Id = wsp.Id,
            PermissionId = wsp.PermissionId,
            PermissionName = wsp.Permission?.Name ?? string.Empty,
            CanExecute = wsp.CanExecute,
            CanModify = wsp.CanModify,
            CanView = wsp.CanView
        });

        return Ok(dtos);
    }

    // POST: api/workflows/{workflowId}/steps/{stepId}/permissions
    [HttpPost("{stepId}/permissions")]
    public async Task<IActionResult> SetStepPermission(int workflowId, int stepId, SetWorkflowStepPermissionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var step = await _context.WorkflowSteps.FindAsync(stepId);
        if (step == null || step.WorkflowId != workflowId) return NotFound();

        // Check if user can manage the workflow
        var canManage = await _stepService.ValidateStepPermissionsAsync(stepId, "modify");
        if (!canManage) return Forbid();

        var existingPermission = await _context.WorkflowStepPermissions
            .FirstOrDefaultAsync(wsp => wsp.WorkflowStepId == stepId && wsp.PermissionId == dto.PermissionId);

        if (existingPermission != null)
        {
            existingPermission.CanExecute = dto.CanExecute;
            existingPermission.CanModify = dto.CanModify;
            existingPermission.CanView = dto.CanView;
        }
        else
        {
            var stepPermission = new WorkflowStepPermission
            {
                WorkflowStepId = stepId,
                PermissionId = dto.PermissionId,
                CanExecute = dto.CanExecute,
                CanModify = dto.CanModify,
                CanView = dto.CanView
            };
            _context.WorkflowStepPermissions.Add(stepPermission);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}