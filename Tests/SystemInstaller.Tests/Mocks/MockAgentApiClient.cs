using SystemInstaller.Application.Interfaces;

namespace SystemInstaller.Tests.Mocks;

public class MockAgentApiClient : IAgentApiClient
{
    public Task<bool> StartInstallationAsync(Guid taskId, string environment)
    {
        // Mock always returns true for successful start
        return Task.FromResult(true);
    }

    public Task<bool> CancelInstallationAsync(Guid taskId)
    {
        // Mock always returns true for successful cancel
        return Task.FromResult(true);
    }

    public Task<string> GetInstallationStatusAsync(Guid taskId)
    {
        // Mock returns a basic status
        return Task.FromResult("Running");
    }
}
