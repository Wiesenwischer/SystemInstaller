using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Installations;

/// <summary>
/// Repository interface for Installation aggregate
/// </summary>
public interface IInstallationRepository : IRepository<Installation, Guid>
{
    Task<List<Installation>> GetByEnvironmentIdAsync(Guid environmentId, CancellationToken cancellationToken = default);
    Task<List<Installation>> GetByStatusAsync(InstallationStatus status, CancellationToken cancellationToken = default);
    Task<Installation?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for InstallationEnvironment
/// </summary>
public interface IInstallationEnvironmentRepository
{
    Task<InstallationEnvironment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<InstallationEnvironment>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<InstallationEnvironment>> GetActiveEnvironmentsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(InstallationEnvironment environment, CancellationToken cancellationToken = default);
    Task UpdateAsync(InstallationEnvironment environment, CancellationToken cancellationToken = default);
    Task DeleteAsync(InstallationEnvironment environment, CancellationToken cancellationToken = default);
}
