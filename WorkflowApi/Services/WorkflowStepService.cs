using Microsoft.EntityFrameworkCore;
using WorkflowApi.Data;
using WorkflowApi.Models;

namespace WorkflowApi.Services;

public class WorkflowStepService
{
    private readonly WorkflowContext _context;
    private readonly AuthorizationService _authorizationService;

    public WorkflowStepService(WorkflowContext context, AuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<WorkflowStep> AddStepAsync(int workflowId, string stepName, StepType stepType, bool isRequired, string description, int order)
    {
        // Check if user can manage the workflow
        var workflow = await _context.Workflows.FindAsync(workflowId);
        if (workflow == null) throw new ArgumentException("Workflow not found");

        var canManage = await _authorizationService.HasPermissionAsync($"ManageWorkflow{workflowId}");
        if (!canManage) throw new UnauthorizedAccessException("User does not have permission to manage this workflow");

        var step = new WorkflowStep
        {
            WorkflowId = workflowId,
            StepName = stepName,
            StepType = stepType,
            IsRequired = isRequired,
            Description = description,
            Order = order
        };

        _context.WorkflowSteps.Add(step);
        await _context.SaveChangesAsync();

        return step;
    }

    public async Task<WorkflowStep> UpdateStepAsync(int stepId, string stepName, StepType stepType, bool isRequired, string description, int order)
    {
        var step = await _context.WorkflowSteps.FindAsync(stepId);
        if (step == null) throw new ArgumentException("Step not found");

        // Check if user can manage the workflow
        var canManage = await _authorizationService.HasPermissionAsync($"ManageWorkflow{step.WorkflowId}");
        if (!canManage) throw new UnauthorizedAccessException("User does not have permission to manage this workflow");

        step.StepName = stepName;
        step.StepType = stepType;
        step.IsRequired = isRequired;
        step.Description = description;
        step.Order = order;

        await _context.SaveChangesAsync();
        return step;
    }

    public async Task<bool> RemoveStepAsync(int stepId)
    {
        var step = await _context.WorkflowSteps.FindAsync(stepId);
        if (step == null) return false;

        // Check if user can manage the workflow
        var canManage = await _authorizationService.HasPermissionAsync($"ManageWorkflow{step.WorkflowId}");
        if (!canManage) throw new UnauthorizedAccessException("User does not have permission to manage this workflow");

        _context.WorkflowSteps.Remove(step);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<WorkflowStep>> GetStepsForWorkflowAsync(int workflowId)
    {
        // Check if user can view the workflow
        var canView = await _authorizationService.HasPermissionAsync($"ViewWorkflow{workflowId}");
        if (!canView) throw new UnauthorizedAccessException("User does not have permission to view this workflow");

        return await _context.WorkflowSteps
            .Where(ws => ws.WorkflowId == workflowId)
            .Include(ws => ws.WorkflowStepPermissions)
            .ThenInclude(wsp => wsp.Permission)
            .OrderBy(ws => ws.Order)
            .ToListAsync();
    }

    public async Task<bool> ValidateStepPermissionsAsync(int stepId, string action)
    {
        var userId = _authorizationService.GetCurrentUserId();
        if (userId == null) return false;

        var step = await _context.WorkflowSteps
            .Include(ws => ws.WorkflowStepPermissions)
            .ThenInclude(wsp => wsp.Permission)
            .FirstOrDefaultAsync(ws => ws.Id == stepId);

        if (step == null) return false;

        // Check if user has any of the permissions for this step
        var userPermissions = await _authorizationService.GetUserPermissionsAsync();

        foreach (var stepPermission in step.WorkflowStepPermissions)
        {
            if (userPermissions.Contains(stepPermission.Permission.Name))
            {
                switch (action.ToLower())
                {
                    case "execute":
                        return stepPermission.CanExecute;
                    case "modify":
                        return stepPermission.CanModify;
                    case "view":
                        return stepPermission.CanView;
                }
            }
        }

        return false;
    }
}