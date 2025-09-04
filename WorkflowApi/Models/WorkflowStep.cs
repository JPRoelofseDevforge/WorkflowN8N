using System;

namespace WorkflowApi.Models;

public enum StepType
{
    Action,
    Condition,
    Trigger,
    Approval,
    Notification
}

public class WorkflowStep
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public StepType StepType { get; set; }
    public bool IsRequired { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? RequiredPermissionId { get; set; }

    // Navigation properties
    public Workflow? Workflow { get; set; }
    public Permission? RequiredPermission { get; set; }
    public ICollection<WorkflowStepPermission> WorkflowStepPermissions { get; set; } = new List<WorkflowStepPermission>();
}