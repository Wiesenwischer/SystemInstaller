using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.IntegrationTests.Mocks;
using SystemInstaller.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using SystemInstaller.Domain.Enums;

namespace SystemInstaller.IntegrationTests.ComponentTests;

public class HomeComponentTests : BlazorComponentTestBase
{
    public HomeComponentTests()
    {
        // Setup authentication
        var mockUser = MockUsers.CreateAuthenticatedUser();
        var authStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
    }

    [Fact]
    public async Task Home_ShouldDisplayWelcomeMessage()
    {
        // Act
        var component = RenderComponent<Home>();

        // Assert
        Assert.Contains("Welcome to System Installer", component.Markup);
    }

    [Fact]
    public async Task Home_ShouldDisplayInstallationStatistics()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        
        var pendingInstallation = TestDataFactory.CreateInstallation(environment.Id);
        pendingInstallation.Status = InstallationStatus.Pending;
        
        var runningInstallation = TestDataFactory.CreateInstallation(environment.Id);
        runningInstallation.Status = InstallationStatus.Running;
        
        var completedInstallation = TestDataFactory.CreateInstallation(environment.Id);
        completedInstallation.Status = InstallationStatus.Completed;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddRangeAsync(pendingInstallation, runningInstallation, completedInstallation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Home>();

        // Assert
        Assert.Contains("Installation Statistics", component.Markup);
        Assert.Contains("3", component.Markup); // Total installations
    }

    [Fact]
    public async Task Home_ShouldDisplayRecentInstallations()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id, "2.1.0");
        installation.CreatedAt = DateTime.UtcNow.AddHours(-1); // Recent

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Home>();

        // Assert
        Assert.Contains("Recent Installations", component.Markup);
        Assert.Contains("2.1.0", component.Markup);
    }

    [Fact]
    public async Task Home_ShouldDisplayActiveEnvironments()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var activeEnvironment = TestDataFactory.CreateEnvironment(tenant.Id, "Production");
        activeEnvironment.IsActive = true;
        
        var inactiveEnvironment = TestDataFactory.CreateEnvironment(tenant.Id, "Staging");
        inactiveEnvironment.IsActive = false;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddRangeAsync(activeEnvironment, inactiveEnvironment);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Home>();

        // Assert
        Assert.Contains("Active Environments", component.Markup);
        Assert.Contains("Production", component.Markup);
        Assert.DoesNotContain("Staging", component.Markup);
    }

    [Fact]
    public async Task Home_ShouldDisplayQuickActionButtons()
    {
        // Act
        var component = RenderComponent<Home>();

        // Assert
        var installationsLink = component.Find("a[href='/installations']");
        var environmentsLink = component.Find("a[href='/environments']");
        
        Assert.NotNull(installationsLink);
        Assert.NotNull(environmentsLink);
    }

    [Fact]
    public async Task Home_ShouldHandleEmptyData()
    {
        // Act
        var component = RenderComponent<Home>();

        // Assert - Should render without errors even with no data
        Assert.NotNull(component);
        Assert.Contains("Welcome to System Installer", component.Markup);
    }

    [Fact]
    public async Task Home_ShouldDisplayInstallationStatusBreakdown()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        
        var installations = new[]
        {
            TestDataFactory.CreateInstallation(environment.Id),
            TestDataFactory.CreateInstallation(environment.Id),
            TestDataFactory.CreateInstallation(environment.Id)
        };
        
        installations[0].Status = InstallationStatus.Pending;
        installations[1].Status = InstallationStatus.Running;
        installations[2].Status = InstallationStatus.Completed;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddRangeAsync(installations);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Home>();

        // Assert
        Assert.Contains("Pending", component.Markup);
        Assert.Contains("Running", component.Markup);
        Assert.Contains("Completed", component.Markup);
    }

    [Fact]
    public async Task Home_ShouldShowEnvironmentCount()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environments = TestDataFactory.CreateMultipleEnvironments(tenant.Id, 5);

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddRangeAsync(environments);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Home>();

        // Assert
        Assert.Contains("5", component.Markup); // Environment count
    }
}
