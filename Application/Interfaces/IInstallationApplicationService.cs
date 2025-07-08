using SystemInstaller.Application.DTOs;

namespace SystemInstaller.Application.Interfaces;

public interface IInstallationApplicationService
{
    Task<List<InstallationEnvironmentResultDto>> GetEnvironmentsAsync(Guid tenantId);
    Task<InstallationEnvironmentResultDto> CreateEnvironmentAsync(CreateInstallationEnvironmentDto dto);
    Task<InstallationTaskResultDto> CreateTaskAsync(CreateInstallationTaskDto dto);
    Task<List<InstallationTaskResultDto>> GetAllTasksAsync(Guid? environmentId = null);
    Task<InstallationTaskResultDto?> GetTaskAsync(Guid taskId);
    Task<bool> StartInstallationAsync(Guid taskId);
    Task<bool> CancelInstallationAsync(Guid taskId);
    event EventHandler<InstallationTaskResultDto>? TaskUpdated;
}
