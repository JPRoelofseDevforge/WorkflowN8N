namespace WorkflowApi.Models;

public class UpdateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Permission fields
    public int? ViewPermissionId { get; set; }
    public int? EditPermissionId { get; set; }
    public int? ExecutePermissionId { get; set; }
    public int? ManagePermissionId { get; set; }
}