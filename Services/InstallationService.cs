using SystemInstaller.Infrastructure.Adapters;
using SystemInstaller.Web.Data;

namespace SystemInstaller.Web.Services;

public class InstallationService
{
    private readonly LegacyInstallationServiceAdapter _adapter;

    public event EventHandler<InstallationTask>? TaskUpdated;

    public InstallationService(LegacyInstallationServiceAdapter adapter)
    {
        _adapter = adapter;
        
        // Forward events from adapter
        _adapter.TaskUpdated += (sender, task) => TaskUpdated?.Invoke(this, task);
    }

    public async Task<List<InstallationEnvironment>> GetEnvironmentsAsync()
    {
        return await _adapter.GetEnvironmentsAsync();
    }

    public async Task AddEnvironmentAsync(InstallationEnvironment env)
    {
        await _adapter.AddEnvironmentAsync(env);
    }

    public IEnumerable<InstallationTask> GetAllTasks()
    {
        return _adapter.GetAllTasks();
    }

    public InstallationTask? GetTask(Guid id)
    {
        return _adapter.GetTask(id);
    }

    public async Task<InstallationTask> CreateTaskAsync(string name, string description, Guid environmentId)
    {
        return await _adapter.CreateTaskAsync(name, description, environmentId);
    }

    public async Task<bool> StartInstallationAsync(Guid taskId)
    {
        return await _adapter.StartInstallationAsync(taskId);
    }

    public async Task<bool> CancelInstallationAsync(Guid taskId)
    {
        return await _adapter.CancelInstallationAsync(taskId);
    }
}
