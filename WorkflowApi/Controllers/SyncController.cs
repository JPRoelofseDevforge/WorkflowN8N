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
public class SyncController : ControllerBase
{
    private readonly WorkflowContext _context;
    private readonly N8nService _n8nService;

    public SyncController(WorkflowContext context, N8nService n8nService)
    {
        _context = context;
        _n8nService = n8nService;
    }

    // POST: api/sync
    [HttpPost]
    [Authorize]
    [RequireRole("Admin")]
    public async Task<IActionResult> SyncWorkflows()
    {
        try
        {
            var n8nWorkflows = await _n8nService.GetWorkflowsAsync();
            var syncedCount = 0;
            var updatedCount = 0;

            foreach (var n8nWorkflow in n8nWorkflows)
            {
                var existingWorkflow = await _context.Workflows
                    .FirstOrDefaultAsync(w => w.N8nId == n8nWorkflow.Id);

                if (existingWorkflow == null)
                {
                    // Create new workflow in database
                    var workflow = new Workflow
                    {
                        Name = n8nWorkflow.Name,
                        Description = $"Synced from n8n: {n8nWorkflow.Name}",
                        IsActive = n8nWorkflow.Active,
                        N8nId = n8nWorkflow.Id,
                        CreatedAt = DateTime.Parse(n8nWorkflow.CreatedAt)
                    };
                    _context.Workflows.Add(workflow);
                    syncedCount++;
                }
                else
                {
                    // Update existing workflow
                    existingWorkflow.Name = n8nWorkflow.Name;
                    existingWorkflow.IsActive = n8nWorkflow.Active;
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Workflows synchronized successfully",
                synced = syncedCount,
                updated = updatedCount,
                total = n8nWorkflows.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error synchronizing workflows: {ex.Message}");
        }
    }

    // POST: api/sync/workflow/{n8nId}
    [HttpPost("workflow/{n8nId}")]
    [Authorize]
    [RequireRole("Admin")]
    public async Task<IActionResult> SyncWorkflow(string n8nId)
    {
        try
        {
            var n8nWorkflows = await _n8nService.GetWorkflowsAsync();
            var workflowData = n8nWorkflows.FirstOrDefault(w => w.Id == n8nId);

            if (workflowData == null)
            {
                return NotFound($"Workflow with n8n ID {n8nId} not found in n8n");
            }

            var existingWorkflow = await _context.Workflows
                .FirstOrDefaultAsync(w => w.N8nId == n8nId);

            if (existingWorkflow == null)
            {
                // Create new workflow
                var workflow = new Workflow
                {
                    Name = workflowData.Name,
                    Description = $"Synced from n8n: {workflowData.Name}",
                    IsActive = workflowData.Active,
                    N8nId = workflowData.Id,
                    CreatedAt = DateTime.Parse(workflowData.CreatedAt)
                };
                _context.Workflows.Add(workflow);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Workflow synced successfully", action = "created", workflowId = workflow.Id });
            }
            else
            {
                // Update existing workflow
                existingWorkflow.Name = workflowData.Name;
                existingWorkflow.IsActive = workflowData.Active;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Workflow updated successfully", action = "updated", workflowId = existingWorkflow.Id });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error synchronizing workflow: {ex.Message}");
        }
    }

    // GET: api/sync/status
    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            var dbWorkflows = await _context.Workflows.ToListAsync();
            var n8nWorkflows = await _n8nService.GetWorkflowsAsync();

            var syncedWorkflows = dbWorkflows.Where(w => !string.IsNullOrEmpty(w.N8nId)).ToList();
            var unsyncedWorkflows = dbWorkflows.Where(w => string.IsNullOrEmpty(w.N8nId)).ToList();

            return Ok(new
            {
                database = new
                {
                    total = dbWorkflows.Count,
                    synced = syncedWorkflows.Count,
                    unsynced = unsyncedWorkflows.Count
                },
                n8n = new
                {
                    total = n8nWorkflows.Count
                },
                syncCandidates = n8nWorkflows.Count - syncedWorkflows.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting sync status: {ex.Message}");
        }
    }
}