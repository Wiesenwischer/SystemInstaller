using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.IntegrationTests.Mocks;
using SystemInstaller.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using SystemInstaller.Domain.Enums;

namespace SystemInstaller.IntegrationTests.ComponentTests;

public class InstallationDetailsComponentTests : BlazorComponentTestBase
{
    public InstallationDetailsComponentTests()
    {
        // Setup authentication
        var mockUser = MockUsers.CreateAuthenticatedUser();
        var authStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayInstallationInformation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id, "Production");
        var installation = TestDataFactory.CreateInstallation(environment.Id, "3.1.0");
        installation.Status = InstallationStatus.Running;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        Assert.Contains("3.1.0", component.Markup);
        Assert.Contains("Running", component.Markup);
        Assert.Contains("Production", component.Markup);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayInstallationTasks()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id);
        
        var task1 = TestDataFactory.CreateInstallationTask(installation.Id, "Download Package");
        task1.Status = InstallationStatus.Completed;
        task1.Order = 1;
        
        var task2 = TestDataFactory.CreateInstallationTask(installation.Id, "Install Dependencies");
        task2.Status = InstallationStatus.Running;
        task2.Order = 2;
        
        var task3 = TestDataFactory.CreateInstallationTask(installation.Id, "Configure Services");
        task3.Status = InstallationStatus.Pending;
        task3.Order = 3;

        installation.Tasks = new List<SystemInstaller.Domain.Entities.InstallationTask> { task1, task2, task3 };

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.InstallationTasks.AddRangeAsync(task1, task2, task3);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        Assert.Contains("Download Package", component.Markup);
        Assert.Contains("Install Dependencies", component.Markup);
        Assert.Contains("Configure Services", component.Markup);
        
        Assert.Contains("Completed", component.Markup);
        Assert.Contains("Running", component.Markup);
        Assert.Contains("Pending", component.Markup);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayProgressBar()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id);
        
        var completedTask = TestDataFactory.CreateInstallationTask(installation.Id, "Completed Task");
        completedTask.Status = InstallationStatus.Completed;
        
        var runningTask = TestDataFactory.CreateInstallationTask(installation.Id, "Running Task");
        runningTask.Status = InstallationStatus.Running;
        
        var pendingTask = TestDataFactory.CreateInstallationTask(installation.Id, "Pending Task");
        pendingTask.Status = InstallationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.InstallationTasks.AddRangeAsync(completedTask, runningTask, pendingTask);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        var progressBar = component.Find(".progress");
        Assert.NotNull(progressBar);
    }

    [Fact]
    public async Task InstallationDetails_ShouldHandleInvalidInstallationId()
    {
        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, Guid.NewGuid().ToString()));

        // Assert
        Assert.Contains("Installation not found", component.Markup);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayStartButton_WhenPending()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id);
        installation.Status = InstallationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        var startButton = component.Find("button:contains('Start Installation')");
        Assert.NotNull(startButton);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayCancelButton_WhenRunning()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id);
        installation.Status = InstallationStatus.Running;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        var cancelButton = component.Find("button:contains('Cancel')");
        Assert.NotNull(cancelButton);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayRetryButton_WhenFailed()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id);
        installation.Status = InstallationStatus.Failed;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        var retryButton = component.Find("button:contains('Retry')");
        Assert.NotNull(retryButton);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayTaskTimestamps()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id);
        
        var task = TestDataFactory.CreateInstallationTask(installation.Id, "Timestamped Task");
        task.StartedAt = DateTime.UtcNow.AddMinutes(-10);
        task.CompletedAt = DateTime.UtcNow.AddMinutes(-5);
        task.Status = InstallationStatus.Completed;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.InstallationTasks.AddAsync(task);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        Assert.Contains("Timestamped Task", component.Markup);
        // Timestamps should be displayed in some format
        Assert.Contains("Started", component.Markup);
        Assert.Contains("Completed", component.Markup);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayEnvironmentInformation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Test Tenant");
        var environment = TestDataFactory.CreateEnvironment(tenant.Id, "Staging Environment");
        environment.ServerUrl = "https://staging.example.com";
        var installation = TestDataFactory.CreateInstallation(environment.Id);

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        Assert.Contains("Staging Environment", component.Markup);
        Assert.Contains("https://staging.example.com", component.Markup);
        Assert.Contains("Test Tenant", component.Markup);
    }

    [Fact]
    public async Task InstallationDetails_ShouldDisplayTasksInCorrectOrder()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation = TestDataFactory.CreateInstallation(environment.Id);
        
        var task1 = TestDataFactory.CreateInstallationTask(installation.Id, "First Task");
        task1.Order = 1;
        
        var task2 = TestDataFactory.CreateInstallationTask(installation.Id, "Second Task");
        task2.Order = 2;
        
        var task3 = TestDataFactory.CreateInstallationTask(installation.Id, "Third Task");
        task3.Order = 3;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.InstallationTasks.AddRangeAsync(task3, task1, task2); // Add in wrong order
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<InstallationDetails>(parameters => parameters
            .Add(p => p.InstallationId, installation.Id.ToString()));

        // Assert
        var markup = component.Markup;
        var firstIndex = markup.IndexOf("First Task");
        var secondIndex = markup.IndexOf("Second Task");
        var thirdIndex = markup.IndexOf("Third Task");
        
        Assert.True(firstIndex < secondIndex);
        Assert.True(secondIndex < thirdIndex);
    }
}
