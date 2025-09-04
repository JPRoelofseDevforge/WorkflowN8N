namespace WorkflowApi.Models;

public class WorkflowStepPermissionDto
{
    public int Id { get; set; }
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public bool CanExecute { get; set; }
    public bool CanModify { get; set; }
    public bool CanView { get; set; }
}