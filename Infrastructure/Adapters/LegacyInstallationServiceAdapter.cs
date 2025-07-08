using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Web.Data;

namespace SystemInstaller.Infrastructure.Adapters;

/// <summary>
/// Legacy adapter for InstallationService to gradually migrate to new architecture
/// </summary>
public class LegacyInstallationServiceAdapter
{
    private readonly IInstallationApplicationService _installationService;
    private readonly List<InstallationTask> _legacyTasks = new(); // In-memory cache for compatibility

    public event EventHandler<InstallationTask>? TaskUpdated;

    public LegacyInstallationServiceAdapter(IInstallationApplicationService installationService)
    {
        _installationService = installationService;
        
        // Subscribe to new service events and convert to legacy events
        _installationService.TaskUpdated += (sender, taskDto) =>
        {
            var legacyTask = MapToLegacyTask(taskDto);
            
            // Update or add to legacy cache
            var existingIndex = _legacyTasks.FindIndex(t => t.Id == legacyTask.Id);
            if (existingIndex >= 0)
            {
                _legacyTasks[existingIndex] = legacyTask;
            }
            else
            {
                _legacyTasks.Add(legacyTask);
            }
            
            TaskUpdated?.Invoke(this, legacyTask);
        };
    }

    public async Task<List<InstallationEnvironment>> GetEnvironmentsAsync()
    {
        // For simplicity, get all environments (would need tenant context in real implementation)
        var environments = await _installationService.GetEnvironmentsAsync(Guid.Empty); // This needs proper tenant context
        
        return environments.Select(e => new InstallationEnvironment
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CreatedAt = e.CreatedAt
        }).ToList();
    }

    public async Task AddEnvironmentAsync(InstallationEnvironment env)
    {
        var dto = new CreateInstallationEnvironmentDto
        {
            TenantId = Guid.Empty, // This needs proper tenant context
            Name = env.Name,
            Description = env.Description
        };

        await _installationService.CreateEnvironmentAsync(dto);
    }

    public IEnumerable<InstallationTask> GetAllTasks()
    {
        return _legacyTasks.OrderByDescending(t => t.CreatedAt);
    }

    public InstallationTask? GetTask(Guid id)
    {
        return _legacyTasks.FirstOrDefault(t => t.Id == id);
    }

    public async Task<InstallationTask> CreateTaskAsync(string name, string description, Guid environmentId)
    {
        var dto = new CreateInstallationTaskDto
        {
            Name = name,
            Description = description,
            EnvironmentId = environmentId
        };

        var result = await _installationService.CreateTaskAsync(dto);
        var legacyTask = MapToLegacyTask(result);
        
        _legacyTasks.Add(legacyTask);
        return legacyTask;
    }

    public async Task<bool> StartInstallationAsync(Guid taskId)
    {
        return await _installationService.StartInstallationAsync(taskId);
    }

    public async Task<bool> CancelInstallationAsync(Guid taskId)
    {
        return await _installationService.CancelInstallationAsync(taskId);
    }

    private static InstallationTask MapToLegacyTask(InstallationTaskResultDto dto)
    {
        return new InstallationTask
        {
            Id = dto.Id,
            EnvironmentId = dto.EnvironmentId,
            Name = dto.Name,
            Description = dto.Description,
            Status = (SystemInstaller.Web.Data.InstallationStatus)dto.Status, // Explicit cast
            CreatedAt = dto.CreatedAt,
            StartedAt = dto.StartedAt,
            CompletedAt = dto.CompletedAt,
            ErrorMessage = dto.ErrorMessage,
            Progress = dto.Progress,
            Logs = dto.Logs
        };
    }
}
