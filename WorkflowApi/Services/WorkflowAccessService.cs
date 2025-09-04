using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkflowApi.Data;
using WorkflowApi.Models;

namespace WorkflowApi.Services;

public class WorkflowAccessService
{
    private readonly WorkflowContext _context;
    private readonly AuthorizationService _authorizationService;

    public WorkflowAccessService(WorkflowContext context, AuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<bool> CanViewWorkflowAsync(int workflowId)
    {
        // Check if user is admin
        if (await _authorizationService.HasRoleAsync("Admin"))
            return true;

        var workflow = await _context.Workflows
            .Include(w => w.ViewPermission)
            .Include(w => w.ManagePermission)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (workflow == null) return false;

        // If no view permission required, allow access
        if (workflow.ViewPermission == null) return true;

        // Check if user has the view permission or manage permission (inheritance)
        return await _authorizationService.HasPermissionAsync(workflow.ViewPermission.Name) ||
               (workflow.ManagePermission != null && await _authorizationService.HasPermissionAsync(workflow.ManagePermission.Name));
    }

    public async Task<bool> CanEditWorkflowAsync(int workflowId)
    {
        // Check if user is admin
        if (await _authorizationService.HasRoleAsync("Admin"))
            return true;

        var workflow = await _context.Workflows
            .Include(w => w.EditPermission)
            .Include(w => w.ManagePermission)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (workflow == null) return false;

        // If no edit permission required, allow access
        if (workflow.EditPermission == null) return true;

        // Check if user has the edit permission or manage permission (inheritance)
        return await _authorizationService.HasPermissionAsync(workflow.EditPermission.Name) ||
               (workflow.ManagePermission != null && await _authorizationService.HasPermissionAsync(workflow.ManagePermission.Name));
    }

    public async Task<bool> CanExecuteWorkflowAsync(int workflowId)
    {
        // Check if user is admin
        if (await _authorizationService.HasRoleAsync("Admin"))
            return true;

        var workflow = await _context.Workflows
            .Include(w => w.ExecutePermission)
            .Include(w => w.ManagePermission)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (workflow == null) return false;

        // If no execute permission required, allow access
        if (workflow.ExecutePermission == null) return true;

        // Check if user has the execute permission or manage permission (inheritance)
        return await _authorizationService.HasPermissionAsync(workflow.ExecutePermission.Name) ||
               (workflow.ManagePermission != null && await _authorizationService.HasPermissionAsync(workflow.ManagePermission.Name));
    }

    public async Task<bool> CanManageWorkflowAsync(int workflowId)
    {
        // Check if user is admin
        if (await _authorizationService.HasRoleAsync("Admin"))
            return true;

        var workflow = await _context.Workflows
            .Include(w => w.ManagePermission)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (workflow == null) return false;

        // If no manage permission required, allow access
        if (workflow.ManagePermission == null) return true;

        // Check if user has the manage permission
        return await _authorizationService.HasPermissionAsync(workflow.ManagePermission.Name);
    }

    public async Task<List<Workflow>> FilterWorkflowsByPermissionsAsync()
    {
        // Get all workflows with permissions
        var workflows = await _context.Workflows
            .Include(w => w.ViewPermission)
            .Include(w => w.EditPermission)
            .Include(w => w.ExecutePermission)
            .Include(w => w.ManagePermission)
            .Include(w => w.WorkflowSteps)
            .ThenInclude(ws => ws.RequiredPermission)
            .ToListAsync();

        var filteredWorkflows = new List<Workflow>();

        foreach (var workflow in workflows)
        {
            if (await CanViewWorkflowAsync(workflow.Id))
            {
                filteredWorkflows.Add(workflow);
            }
        }

        return filteredWorkflows;
    }
}