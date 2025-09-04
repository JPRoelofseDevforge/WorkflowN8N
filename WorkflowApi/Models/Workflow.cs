using System;
using System.Collections.Generic;

namespace WorkflowApi.Models;

public class Workflow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? N8nId { get; set; }
    public int? CreatedById { get; set; }

    // Permission foreign keys
    public int? ViewPermissionId { get; set; }
    public int? EditPermissionId { get; set; }
    public int? ExecutePermissionId { get; set; }
    public int? ManagePermissionId { get; set; }

    // Navigation properties
    public Permission? ViewPermission { get; set; }
    public Permission? EditPermission { get; set; }
    public Permission? ExecutePermission { get; set; }
    public Permission? ManagePermission { get; set; }

    public User? CreatedBy { get; set; }
    public ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
    public ICollection<Execution> Executions { get; set; } = new List<Execution>();
}