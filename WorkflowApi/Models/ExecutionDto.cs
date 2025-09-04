using System;

namespace WorkflowApi.Models;

public class ExecutionDto
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}