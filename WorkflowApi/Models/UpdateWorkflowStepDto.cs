using System.ComponentModel.DataAnnotations;

namespace WorkflowApi.Models;

public class UpdateWorkflowStepDto
{
    [Required]
    public string StepName { get; set; } = string.Empty;

    [Required]
    public StepType StepType { get; set; }

    public bool IsRequired { get; set; }

    public string Description { get; set; } = string.Empty;

    [Required]
    public int Order { get; set; }
}