using System;

namespace WorkflowApi.Models;

public class WorkflowStepPermission
{
    public int Id { get; set; }
    public int WorkflowStepId { get; set; }
    public int PermissionId { get; set; }
    public bool CanExecute { get; set; }
    public bool CanModify { get; set; }
    public bool CanView { get; set; }

    // Navigation properties
    public WorkflowStep? WorkflowStep { get; set; }
    public Permission? Permission { get; set; }
}