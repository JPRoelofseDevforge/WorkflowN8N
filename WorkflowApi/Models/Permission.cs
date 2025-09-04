using System;
using System.Collections.Generic;

namespace WorkflowApi.Models;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}