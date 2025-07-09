namespace SystemInstaller.Application.Interfaces;

public interface IAgentApiClient
{
    Task<bool> StartInstallationAsync(Guid taskId, string environment);
    Task<bool> CancelInstallationAsync(Guid taskId);
    Task<string> GetInstallationStatusAsync(Guid taskId);
}
