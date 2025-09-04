using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowApi.Data;
using WorkflowApi.Models;

namespace WorkflowApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionsController : ControllerBase
{
    private readonly WorkflowContext _context;

    public ExecutionsController(WorkflowContext context)
    {
        _context = context;
    }

    // GET: api/executions
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ExecutionDto>>> GetExecutions()
    {
        var executions = await _context.Executions.ToListAsync();
        var dtos = executions.Select(e => new ExecutionDto
        {
            Id = e.Id,
            WorkflowId = e.WorkflowId,
            Status = e.Status,
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt
        });
        return Ok(dtos);
    }

    // GET: api/executions/{n8nId}
    [HttpGet("{n8nId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ExecutionDto>>> GetExecutionsByWorkflow(string n8nId)
    {
        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.N8nId == n8nId);
        if (workflow == null) return NotFound();
        var executions = await _context.Executions.Where(e => e.WorkflowId == workflow.Id).ToListAsync();
        var dtos = executions.Select(e => new ExecutionDto
        {
            Id = e.Id,
            WorkflowId = e.WorkflowId,
            Status = e.Status,
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt
        });
        return Ok(dtos);
    }
}