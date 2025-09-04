using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkflowApi.Models;

namespace WorkflowApi.Services;

public class N8nService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<N8nService> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public N8nService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<N8nService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
        _baseUrl = _configuration["N8n:BaseUrl"] ?? throw new ArgumentNullException("N8n:BaseUrl not configured");
        _apiKey = _configuration["N8n:ApiKey"] ?? Environment.GetEnvironmentVariable("N8N_API_KEY") ?? throw new ArgumentNullException("N8n:ApiKey not configured");

        _logger.LogInformation("N8nService: BaseUrl from config: {BaseUrl}", _baseUrl);
        _logger.LogInformation("N8nService: ApiKey configured: {HasApiKey}", !string.IsNullOrEmpty(_apiKey));
        _logger.LogInformation("N8nService: ApiKey from env: {HasEnvApiKey}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("N8N_API_KEY")));

        // Log all N8n related environment variables
        var envVars = Environment.GetEnvironmentVariables();
        foreach (var key in envVars.Keys)
        {
            if (key.ToString().StartsWith("N8n", StringComparison.OrdinalIgnoreCase) || key.ToString().Contains("N8N"))
            {
                _logger.LogInformation("N8nService: Env var {Key} = {Value}", key, envVars[key]);
            }
        }

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-N8N-API-KEY", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _logger.LogInformation("N8nService: HttpClient BaseAddress set to: {BaseAddress}", _httpClient.BaseAddress);
    }

    public async Task<List<N8nWorkflowDto>> GetWorkflowsAsync()
    {
        try
        {
            _logger.LogInformation("N8nService.GetWorkflowsAsync: BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
            _logger.LogInformation("N8nService.GetWorkflowsAsync: Requesting: {Url}", _httpClient.BaseAddress + "workflows");
            var response = await _httpClient.GetAsync("workflows");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("N8nService.GetWorkflowsAsync: Response content: {Content}", content);
            _logger.LogInformation("N8nService.GetWorkflowsAsync: Response content length: {Length}", content.Length);

            // Parse the JSON manually to handle the data wrapper
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
            {
                var workflows = new List<N8nWorkflowDto>();
                foreach (var item in dataElement.EnumerateArray())
                {
                    try
                    {
                        var workflow = JsonSerializer.Deserialize<N8nWorkflowDto>(item.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (workflow != null)
                        {
                            workflows.Add(workflow);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "N8nService.GetWorkflowsAsync: Error deserializing individual workflow");
                        // Continue with other workflows
                    }
                }
                _logger.LogInformation("N8nService.GetWorkflowsAsync: Successfully deserialized {Count} workflows", workflows.Count);
                return workflows;
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                // Response is directly an array
                var workflows = new List<N8nWorkflowDto>();
                foreach (var item in root.EnumerateArray())
                {
                    try
                    {
                        var workflow = JsonSerializer.Deserialize<N8nWorkflowDto>(item.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (workflow != null)
                        {
                            workflows.Add(workflow);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "N8nService.GetWorkflowsAsync: Error deserializing individual workflow from direct array");
                        // Continue with other workflows
                    }
                }
                _logger.LogInformation("N8nService.GetWorkflowsAsync: Successfully deserialized {Count} workflows from direct array", workflows.Count);
                return workflows;
            }
            else
            {
                _logger.LogWarning("N8nService.GetWorkflowsAsync: Unexpected response format");
                return new List<N8nWorkflowDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "N8nService.GetWorkflowsAsync: Error retrieving workflows");
            throw new Exception($"Error retrieving workflows: {ex.Message}", ex);
        }
    }

    public async Task<N8nWorkflowDto> CreateWorkflowAsync(CreateN8nWorkflowDto workflow)
    {
        var json = JsonSerializer.Serialize(workflow, new JsonSerializerOptions { WriteIndented = true });
        _logger.LogInformation("N8nService.CreateWorkflowAsync: Sending payload: {Payload}", json);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("workflows", content);
        _logger.LogInformation("N8nService.CreateWorkflowAsync: Response status: {Status}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("N8nService.CreateWorkflowAsync: Error response: {Error}", errorContent);
            throw new Exception($"Error creating workflow in n8n: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("N8nService.CreateWorkflowAsync: Response: {Response}", responseContent);
        var wrapper = JsonSerializer.Deserialize<N8nWorkflowResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return wrapper?.Data ?? throw new Exception("Failed to deserialize created workflow");
    }

    public async Task<N8nWorkflowDto> UpdateWorkflowAsync(string id, UpdateN8nWorkflowDto workflow)
    {
        try
        {
            var json = JsonSerializer.Serialize(workflow);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"workflows/{id}", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<N8nWorkflowDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? throw new Exception("Failed to deserialize updated workflow");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error updating workflow {id}: {ex.Message}", ex);
        }
    }

    public async Task DeleteWorkflowAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"workflows/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error deleting workflow {id}: {ex.Message}", ex);
        }
    }

    public async Task<N8nWorkflowDto> ToggleWorkflowAsync(string id)
    {
        try
        {
            var response = await _httpClient.PutAsync($"workflows/{id}", null);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<N8nWorkflowDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? throw new Exception("Failed to deserialize toggled workflow");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error toggling workflow {id}: {ex.Message}", ex);
        }
    }

    public async Task<N8nExecutionDto> ExecuteWorkflowAsync(string id, ExecuteN8nWorkflowDto? executionData = null)
    {
        try
        {
            HttpContent? content = null;
            if (executionData != null)
            {
                var json = JsonSerializer.Serialize(executionData);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var response = await _httpClient.PostAsync($"workflows/{id}/execute", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<N8nExecutionDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? throw new Exception("Failed to deserialize execution result");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error executing workflow {id}: {ex.Message}", ex);
        }
    }
}