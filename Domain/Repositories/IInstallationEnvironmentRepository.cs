using SystemInstaller.Domain.Entities;

namespace SystemInstaller.Domain.Repositories;

public interface IInstallationEnvironmentRepository
{
    Task<InstallationEnvironment?> GetByIdAsync(Guid id);
    Task<IEnumerable<InstallationEnvironment>> GetByTenantIdAsync(Guid tenantId);
    Task<InstallationEnvironment> CreateAsync(InstallationEnvironment environment);
    Task AddAsync(InstallationEnvironment environment);
    Task UpdateAsync(InstallationEnvironment environment);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
