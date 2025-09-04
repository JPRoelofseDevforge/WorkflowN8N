using System.ComponentModel.DataAnnotations;

namespace WorkflowApi.Models;

public class SetWorkflowStepPermissionDto
{
    [Required]
    public int PermissionId { get; set; }

    public bool CanExecute { get; set; }
    public bool CanModify { get; set; }
    public bool CanView { get; set; }
}