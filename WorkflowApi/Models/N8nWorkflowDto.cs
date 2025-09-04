using System.Text.Json.Serialization;

namespace WorkflowApi.Models;

public class N8nWorkflowsResponse
{
    [JsonPropertyName("data")]
    public List<N8nWorkflowDto> Data { get; set; } = new List<N8nWorkflowDto>();

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

public class N8nWorkflowResponse
{
    [JsonPropertyName("data")]
    public N8nWorkflowDto Data { get; set; } = new N8nWorkflowDto();
}

public class N8nWorkflowDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("nodes")]
    public object? Nodes { get; set; }

    [JsonPropertyName("connections")]
    public object? Connections { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = string.Empty;

    [JsonPropertyName("settings")]
    public object? Settings { get; set; }

    [JsonPropertyName("staticData")]
    public object? StaticData { get; set; }

    [JsonPropertyName("meta")]
    public object? Meta { get; set; }

    [JsonPropertyName("pinData")]
    public object? PinData { get; set; }

    [JsonPropertyName("versionId")]
    public string VersionId { get; set; } = string.Empty;

    [JsonPropertyName("triggerCount")]
    public int TriggerCount { get; set; }

    [JsonPropertyName("isArchived")]
    public bool IsArchived { get; set; }

    [JsonPropertyName("tags")]
    public object? Tags { get; set; }
}

public class CreateN8nWorkflowDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nodes")]
    public List<object>? Nodes { get; set; } = new List<object>();

    [JsonPropertyName("connections")]
    public object? Connections { get; set; } = new Dictionary<string, object>();

    [JsonPropertyName("settings")]
    public object? Settings { get; set; } = new { executionOrder = "v1" };
}

public class UpdateN8nWorkflowDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nodes")]
    public object? Nodes { get; set; }

    [JsonPropertyName("connections")]
    public object? Connections { get; set; }
}