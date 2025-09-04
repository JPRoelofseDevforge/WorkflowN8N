using System;

namespace WorkflowApi.Models;

public class WorkflowStepDto
{
    public int Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public StepType StepType { get; set; }
    public bool IsRequired { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? RequiredPermissionId { get; set; }
    public string? RequiredPermissionName { get; set; }
}