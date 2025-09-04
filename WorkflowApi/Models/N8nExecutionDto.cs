using System.Text.Json.Serialization;

namespace WorkflowApi.Models;

public class N8nExecutionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workflowId")]
    public string WorkflowId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("finishedAt")]
    public DateTime? FinishedAt { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public class ExecuteN8nWorkflowDto
{
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}