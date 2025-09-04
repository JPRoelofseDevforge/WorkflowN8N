using System;
using System.Collections.Generic;

namespace WorkflowApi.Models;

public class WorkflowDto
{
    public int Id { get; set; }
    public string N8nId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // Permission fields
    public int? ViewPermissionId { get; set; }
    public string? ViewPermissionName { get; set; }
    public int? EditPermissionId { get; set; }
    public string? EditPermissionName { get; set; }
    public int? ExecutePermissionId { get; set; }
    public string? ExecutePermissionName { get; set; }
    public int? ManagePermissionId { get; set; }
    public string? ManagePermissionName { get; set; }

    // Step permissions
    public List<WorkflowStepDto> Steps { get; set; } = new List<WorkflowStepDto>();
}