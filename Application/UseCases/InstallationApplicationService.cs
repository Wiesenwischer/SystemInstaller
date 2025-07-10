using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.ValueObjects;
using SystemInstaller.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace SystemInstaller.Application.UseCases;

public class InstallationApplicationService : IInstallationApplicationService
{
    private readonly IInstallationEnvironmentRepository _environmentRepository;
    private readonly IInstallationTaskRepository _taskRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IAgentApiClient _agentApiClient;
    private readonly ILogger<InstallationApplicationService> _logger;

    public event EventHandler<InstallationTaskResultDto>? TaskUpdated;

    public InstallationApplicationService(
        IInstallationEnvironmentRepository environmentRepository,
        IInstallationTaskRepository taskRepository,
        ITenantRepository tenantRepository,
        IAgentApiClient agentApiClient,
        ILogger<InstallationApplicationService> logger)
    {
        _environmentRepository = environmentRepository;
        _taskRepository = taskRepository;
        _tenantRepository = tenantRepository;
        _agentApiClient = agentApiClient;
        _logger = logger;
    }

    public async Task<List<InstallationEnvironmentResultDto>> GetEnvironmentsAsync(Guid tenantId)
    {
        var environments = await _environmentRepository.GetByTenantIdAsync(tenantId);
        return environments.Select(MapEnvironmentToDto).ToList();
    }

    public async Task<InstallationEnvironmentResultDto> CreateEnvironmentAsync(CreateInstallationEnvironmentDto dto)
    {
        var tenant = await _tenantRepository.GetByIdAsync(dto.TenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found.");
        }

        var environment = new InstallationEnvironment(dto.TenantId, dto.Name, dto.Description);
        await _environmentRepository.AddAsync(environment);

        _logger.LogInformation("Created new environment {EnvironmentId} for tenant {TenantId}", environment.Id, dto.TenantId);

        return MapEnvironmentToDto(environment);
    }

    public async Task<InstallationTaskResultDto> CreateTaskAsync(CreateInstallationTaskDto dto)
    {
        var environment = await _environmentRepository.GetByIdAsync(dto.EnvironmentId);
        if (environment == null)
        {
            throw new InvalidOperationException("Environment not found.");
        }

        var task = new InstallationTask(dto.EnvironmentId, dto.Name, dto.Description);
        await _taskRepository.AddAsync(task);

        _logger.LogInformation("Created new installation task {TaskId} for environment {EnvironmentId}", task.Id, dto.EnvironmentId);

        var result = MapTaskToDto(task, environment.Name);
        TaskUpdated?.Invoke(this, result);
        
        return result;
    }

    public async Task<List<InstallationTaskResultDto>> GetAllTasksAsync(Guid? environmentId = null)
    {
        var tasks = environmentId.HasValue 
            ? await _taskRepository.GetByEnvironmentIdAsync(environmentId.Value)
            : await _taskRepository.GetAllAsync();

        var results = new List<InstallationTaskResultDto>();
        foreach (var task in tasks)
        {
            var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
            results.Add(MapTaskToDto(task, environment?.Name));
        }

        return results.OrderByDescending(t => t.CreatedAt).ToList();
    }

    public async Task<InstallationTaskResultDto?> GetTaskAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return null;

        var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
        return MapTaskToDto(task, environment?.Name);
    }

    public async Task<bool> StartInstallationAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.Status != InstallationStatus.Pending)
        {
            return false;
        }

        try
        {
            var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
            var success = await _agentApiClient.StartInstallationAsync(taskId, environment?.Name ?? "unknown");
            
            if (success)
            {
                task.Start();
                await _taskRepository.UpdateAsync(task);
                
                _logger.LogInformation("Started installation for task {TaskId}", taskId);
                
                var result = MapTaskToDto(task, environment?.Name);
                TaskUpdated?.Invoke(this, result);

                // Simulate installation progress
                _ = Task.Run(() => SimulateInstallationProgressAsync(taskId));
                
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start installation for task {TaskId}", taskId);
            task.Fail(ex.Message);
            await _taskRepository.UpdateAsync(task);
            
            var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
            var result = MapTaskToDto(task, environment?.Name);
            TaskUpdated?.Invoke(this, result);
        }

        return false;
    }

    public async Task<bool> CancelInstallationAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.Status != InstallationStatus.Running)
        {
            return false;
        }

        try
        {
            var success = await _agentApiClient.CancelInstallationAsync(taskId);
            if (success)
            {
                task.Cancel();
                await _taskRepository.UpdateAsync(task);
                
                _logger.LogInformation("Cancelled installation for task {TaskId}", taskId);
                
                var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
                var result = MapTaskToDto(task, environment?.Name);
                TaskUpdated?.Invoke(this, result);
                
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
        var task = await _taskRepository.GetByIdAsync(taskId);
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
                // Refresh task state
                task = await _taskRepository.GetByIdAsync(taskId);
                if (task == null || task.Status != InstallationStatus.Running) break;

                await Task.Delay(2000 + random.Next(1000, 3000)); // Random delay between steps

                var progress = (i + 1) * 100 / steps.Length;
                task.UpdateProgress(progress);
                task.Logs.Add($"{steps[i]}... ({progress}%)");
                await _taskRepository.UpdateAsync(task);
                
                var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
                var result = MapTaskToDto(task, environment?.Name);
                TaskUpdated?.Invoke(this, result);

                // Small chance of failure for demonstration
                if (random.Next(1, 20) == 1)
                {
                    task.Fail($"Failed during: {steps[i]}");
                    await _taskRepository.UpdateAsync(task);
                    
                    result = MapTaskToDto(task, environment?.Name);
                    TaskUpdated?.Invoke(this, result);
                    return;
                }
            }

            // Refresh task state one more time
            task = await _taskRepository.GetByIdAsync(taskId);
            if (task != null && task.Status == InstallationStatus.Running)
            {
                task.Complete();
                await _taskRepository.UpdateAsync(task);
                
                var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
                var result = MapTaskToDto(task, environment?.Name);
                TaskUpdated?.Invoke(this, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during installation simulation for task {TaskId}", taskId);
            task = await _taskRepository.GetByIdAsync(taskId);
            if (task != null)
            {
                task.Fail(ex.Message);
                await _taskRepository.UpdateAsync(task);
                
                var environment = await _environmentRepository.GetByIdAsync(task.EnvironmentId);
                var result = MapTaskToDto(task, environment?.Name);
                TaskUpdated?.Invoke(this, result);
            }
        }
    }

    private static InstallationEnvironmentResultDto MapEnvironmentToDto(InstallationEnvironment environment)
    {
        return new InstallationEnvironmentResultDto
        {
            Id = environment.Id,
            TenantId = environment.TenantId,
            Name = environment.Name,
            Description = environment.Description,
            CreatedAt = environment.CreatedAt,
            UpdatedAt = environment.UpdatedAt,
            Tasks = environment.Tasks.Select(t => MapTaskToDto(t)).ToList()
        };
    }

    private static InstallationTaskResultDto MapTaskToDto(InstallationTask task, string? environmentName = null)
    {
        return new InstallationTaskResultDto
        {
            Id = task.Id,
            EnvironmentId = task.EnvironmentId,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status,
            CreatedAt = task.CreatedAt,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            ErrorMessage = task.ErrorMessage,
            Progress = task.Progress,
            Logs = task.Logs.ToList(),
            EnvironmentName = environmentName
        };
    }
}
