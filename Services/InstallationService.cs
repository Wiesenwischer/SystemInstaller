using SystemInstaller.Web.Data;

namespace SystemInstaller.Web.Services;

using Microsoft.EntityFrameworkCore;
public class InstallationService
{
    private readonly List<InstallationTask> _tasks = new();
    private readonly AgentApiClient _agentApiClient;
    private readonly ILogger<InstallationService> _logger;
    private readonly SystemInstallerDbContext _db;

    public InstallationService(AgentApiClient agentApiClient, ILogger<InstallationService> logger, SystemInstallerDbContext db)
    {
        _agentApiClient = agentApiClient;
        _logger = logger;
        _db = db;
        // SeedSampleData(); // Optional: Entfernen oder anpassen
    }

    public async Task<List<InstallationEnvironment>> GetEnvironmentsAsync()
    {
        return await _db.Environments.AsNoTracking().ToListAsync();
    }

    public async Task AddEnvironmentAsync(InstallationEnvironment env)
    {
        _db.Environments.Add(env);
        await _db.SaveChangesAsync();
    }

    public event EventHandler<InstallationTask>? TaskUpdated;

    public IEnumerable<InstallationTask> GetAllTasks()
    {
        return _tasks.OrderByDescending(t => t.CreatedAt);
    }

    public InstallationTask? GetTask(Guid id)
    {
        return _tasks.FirstOrDefault(t => t.Id == id);
    }

    public async Task<InstallationTask> CreateTaskAsync(string name, string description, Guid environmentId)
    {
        var task = new InstallationTask
        {
            Name = name,
            Description = description,
            EnvironmentId = environmentId
        };

        _tasks.Add(task);
        _logger.LogInformation("Created new installation task {TaskId} for environment {EnvironmentId}", task.Id, environmentId);

        TaskUpdated?.Invoke(this, task);
        return await Task.FromResult(task);
    }

    public async Task<bool> StartInstallationAsync(Guid taskId)
    {
        var task = GetTask(taskId);
        if (task == null || task.Status != InstallationStatus.Pending)
        {
            return false;
        }

        try
        {
            var success = await _agentApiClient.StartInstallationAsync(taskId, task.EnvironmentId.ToString());
            if (success)
            {
                task.Status = InstallationStatus.Running;
                task.StartedAt = DateTime.UtcNow;
                task.Logs.Add($"[{DateTime.UtcNow:HH:mm:ss}] Installation started");
                
                _logger.LogInformation("Started installation for task {TaskId}", taskId);
                TaskUpdated?.Invoke(this, task);

                // Simulate installation progress
                _ = Task.Run(() => SimulateInstallationProgressAsync(taskId));
                
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start installation for task {TaskId}", taskId);
            task.Status = InstallationStatus.Failed;
            task.ErrorMessage = ex.Message;
            TaskUpdated?.Invoke(this, task);
        }

        return false;
    }

    public async Task<bool> CancelInstallationAsync(Guid taskId)
    {
        var task = GetTask(taskId);
        if (task == null || task.Status != InstallationStatus.Running)
        {
            return false;
        }

        try
        {
            var success = await _agentApiClient.CancelInstallationAsync(taskId);
            if (success)
            {
                task.Status = InstallationStatus.Cancelled;
                task.CompletedAt = DateTime.UtcNow;
                task.Logs.Add($"[{DateTime.UtcNow:HH:mm:ss}] Installation cancelled");
                
                _logger.LogInformation("Cancelled installation for task {TaskId}", taskId);
                TaskUpdated?.Invoke(this, task);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel installation for task {TaskId}", taskId);
        }

        return false;
    }

    private async Task SimulateInstallationProgressAsync(Guid taskId)
    {
        var task = GetTask(taskId);
        if (task == null) return;

        var random = new Random();
        var steps = new[]
        {
            "Downloading Docker images",
            "Setting up environment variables",
            "Creating Docker containers",
            "Configuring networking",
            "Starting services",
            "Running health checks",
            "Finalizing installation"
        };

        try
        {
            for (int i = 0; i < steps.Length; i++)
            {
                if (task.Status != InstallationStatus.Running) break;

                await Task.Delay(2000 + random.Next(1000, 3000)); // Random delay between steps

                task.Progress = (i + 1) * 100 / steps.Length;
                task.Logs.Add($"[{DateTime.UtcNow:HH:mm:ss}] {steps[i]}... ({task.Progress}%)");
                
                TaskUpdated?.Invoke(this, task);

                // Small chance of failure for demonstration
                if (random.Next(1, 20) == 1)
                {
                    task.Status = InstallationStatus.Failed;
                    task.ErrorMessage = $"Failed during: {steps[i]}";
                    task.CompletedAt = DateTime.UtcNow;
                    task.Logs.Add($"[{DateTime.UtcNow:HH:mm:ss}] ERROR: {task.ErrorMessage}");
                    TaskUpdated?.Invoke(this, task);
                    return;
                }
            }

            if (task.Status == InstallationStatus.Running)
            {
                task.Status = InstallationStatus.Completed;
                task.Progress = 100;
                task.CompletedAt = DateTime.UtcNow;
                task.Logs.Add($"[{DateTime.UtcNow:HH:mm:ss}] Installation completed successfully");
                TaskUpdated?.Invoke(this, task);
            }
        }
        catch (Exception ex)
        {
            task.Status = InstallationStatus.Failed;
            task.ErrorMessage = ex.Message;
            task.CompletedAt = DateTime.UtcNow;
            task.Logs.Add($"[{DateTime.UtcNow:HH:mm:ss}] ERROR: {ex.Message}");
            TaskUpdated?.Invoke(this, task);
        }
    }

    private void SeedSampleData()
    {
        var sampleTasks = new[]
        {
            new InstallationTask
            {
                Name = "Development Environment Setup",
                Description = "Setting up development environment with Docker containers",
                EnvironmentId = Guid.NewGuid(), // Placeholder - in real app this would be a valid environment ID
                Status = InstallationStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                StartedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(5),
                CompletedAt = DateTime.UtcNow.AddHours(-1),
                Progress = 100,
                Logs = new List<string>
                {
                    $"[{DateTime.UtcNow.AddHours(-2):HH:mm:ss}] Installation started",
                    $"[{DateTime.UtcNow.AddHours(-2).AddMinutes(10):HH:mm:ss}] Downloading Docker images... (25%)",
                    $"[{DateTime.UtcNow.AddHours(-2).AddMinutes(25):HH:mm:ss}] Creating Docker containers... (75%)",
                    $"[{DateTime.UtcNow.AddHours(-1):HH:mm:ss}] Installation completed successfully"
                }
            },
            new InstallationTask
            {
                Name = "Testing Environment Refresh",
                Description = "Updating testing environment with latest configurations",
                EnvironmentId = Guid.NewGuid(), // Placeholder - in real app this would be a valid environment ID
                Status = InstallationStatus.Failed,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                StartedAt = DateTime.UtcNow.AddMinutes(-25),
                CompletedAt = DateTime.UtcNow.AddMinutes(-10),
                Progress = 60,
                ErrorMessage = "Failed during: Configuring networking",
                Logs = new List<string>
                {
                    $"[{DateTime.UtcNow.AddMinutes(-25):HH:mm:ss}] Installation started",
                    $"[{DateTime.UtcNow.AddMinutes(-20):HH:mm:ss}] Downloading Docker images... (20%)",
                    $"[{DateTime.UtcNow.AddMinutes(-15):HH:mm:ss}] Setting up environment variables... (40%)",
                    $"[{DateTime.UtcNow.AddMinutes(-10):HH:mm:ss}] ERROR: Failed during: Configuring networking"
                }
            }
        };

        _tasks.AddRange(sampleTasks);
    }
}