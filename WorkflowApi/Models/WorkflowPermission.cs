using System;

namespace WorkflowApi.Models;

public class WorkflowPermission
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public int PermissionId { get; set; }
    public string PermissionType { get; set; } = string.Empty; // View, Edit, Execute, Manage

    // Navigation properties
    public Workflow? Workflow { get; set; }
    public Permission? Permission { get; set; }
}