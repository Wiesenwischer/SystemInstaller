using SystemInstaller.IntegrationTests.TestBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SystemInstaller.IntegrationTests;

public class BuildSystemTests : BlazorComponentTestBase
{
    [Fact]
    public void DatabaseContext_ShouldBeConfiguredCorrectly()
    {
        // Assert
        Assert.NotNull(DbContext);
        Assert.True(DbContext.Database.IsInMemory());
    }

    [Fact]
    public void WebApplicationFactory_ShouldStartSuccessfully()
    {
        // Assert
        Assert.NotNull(WebApplicationFactory);
        var client = WebApplicationFactory.CreateClient();
        Assert.NotNull(client);
    }

    [Fact]
    public void Services_ShouldBeRegistered()
    {
        // Arrange
        var serviceProvider = WebApplicationFactory.Services;

        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<SystemInstaller.Application.Interfaces.ITenantApplicationService>());
        Assert.NotNull(serviceProvider.GetRequiredService<SystemInstaller.Application.Interfaces.IInstallationApplicationService>());
        Assert.NotNull(serviceProvider.GetRequiredService<SystemInstaller.Application.Interfaces.IUserInvitationApplicationService>());
    }
}
