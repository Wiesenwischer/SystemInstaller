namespace SystemInstaller.Web.Services;

public class AgentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentApiClient> _logger;

    public AgentApiClient(HttpClient httpClient, ILogger<AgentApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> StartInstallationAsync(Guid taskId, string environment)
    {
        _logger.LogInformation("Starting installation for task {TaskId} in environment {Environment}", taskId, environment);
        
        // Stub implementation - will be implemented later to call actual agent API
        await Task.Delay(100); // Simulate API call
        
        return true;
    }

    public async Task<bool> CancelInstallationAsync(Guid taskId)
    {
        _logger.LogInformation("Cancelling installation for task {TaskId}", taskId);
        
        // Stub implementation - will be implemented later to call actual agent API
        await Task.Delay(100); // Simulate API call
        
        return true;
    }

    public async Task<string[]> GetAvailableEnvironmentsAsync()
    {
        _logger.LogInformation("Fetching available environments");
        
        // Stub implementation - will be implemented later to call actual agent API
        await Task.Delay(100); // Simulate API call
        
        return new[] { "Development", "Testing", "Staging", "Production" };
    }
}