using SystemInstaller.Domain.Entities;

namespace SystemInstaller.Domain.Repositories;

public interface IInstallationTaskRepository
{
    Task<InstallationTask?> GetByIdAsync(Guid id);
    Task<IEnumerable<InstallationTask>> GetAllAsync();
    Task<IEnumerable<InstallationTask>> GetByEnvironmentIdAsync(Guid environmentId);
    Task<IEnumerable<InstallationTask>> GetByStatusAsync(Domain.ValueObjects.InstallationStatus status);
    Task<InstallationTask> CreateAsync(InstallationTask task);
    Task AddAsync(InstallationTask task);
    Task UpdateAsync(InstallationTask task);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
