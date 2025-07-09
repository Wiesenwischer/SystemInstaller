using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Application.DTOs;
using SystemInstaller.Domain.Enums;

namespace SystemInstaller.IntegrationTests.ApplicationTests;

public class InstallationApplicationServiceTests : BlazorComponentTestBase
{
    private readonly IInstallationApplicationService _installationService;

    public InstallationApplicationServiceTests()
    {
        _installationService = WebApplicationFactory.Services.GetRequiredService<IInstallationApplicationService>();
    }

    [Fact]
    public async Task GetEnvironmentsAsync_ShouldReturnAllEnvironments()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Environments.AddAsync(environment);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetEnvironmentsAsync(tenant.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(environment.Name, result.First().Name);
    }

    [Fact]
    public async Task CreateEnvironmentAsync_ShouldCreateNewEnvironment()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateInstallationEnvironmentDto
        {
            TenantId = tenant.Id,
            Name = "Test Environment",
            Description = "Test Description"
        };

        // Act
        var result = await _installationService.CreateEnvironmentAsync(createDto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Environment", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(tenant.Id, result.TenantId);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldCreateNewTask()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Environments.AddAsync(environment);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateInstallationTaskDto
        {
            Name = "Test Task",
            Description = "Test Description",
            EnvironmentId = environment.Id
        };

        // Act
        var result = await _installationService.CreateTaskAsync(createDto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Task", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(environment.Id, result.EnvironmentId);
        Assert.Equal(InstallationStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnAllTasks()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var task1 = TestDataFactory.CreateInstallationTask(environment.Id, "Task 1");
        var task2 = TestDataFactory.CreateInstallationTask(environment.Id, "Task 2");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Environments.AddAsync(environment);
        await DbContext.Tasks.AddRangeAsync(task1, task2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetAllTasksAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Name == "Task 1");
        Assert.Contains(result, t => t.Name == "Task 2");
    }

    [Fact]
    public async Task GetTaskAsync_ShouldReturnCorrectTask()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var task = TestDataFactory.CreateInstallationTask(environment.Id, "Test Task");
        task.Status = InstallationStatus.Running;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Environments.AddAsync(environment);
        await DbContext.Tasks.AddAsync(task);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetTaskAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal("Test Task", result.Name);
        Assert.Equal(InstallationStatus.Running, result.Status);
    }

    [Fact]
    public async Task StartInstallationAsync_ShouldStartTask()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var task = TestDataFactory.CreateInstallationTask(environment.Id, "Test Task");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Environments.AddAsync(environment);
        await DbContext.Tasks.AddAsync(task);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.StartInstallationAsync(task.Id);

        // Assert
        Assert.True(result);

        // Verify task status was updated
        var updatedTask = await _installationService.GetTaskAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(InstallationStatus.Running, updatedTask.Status);
    }

    [Fact]
    public async Task CancelInstallationAsync_ShouldCancelTask()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var task = TestDataFactory.CreateInstallationTask(environment.Id, "Test Task");
        task.Status = InstallationStatus.Running;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Environments.AddAsync(environment);
        await DbContext.Tasks.AddAsync(task);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.CancelInstallationAsync(task.Id);

        // Assert
        Assert.True(result);

        // Verify task status was updated
        var updatedTask = await _installationService.GetTaskAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(InstallationStatus.Cancelled, updatedTask.Status);
    }
}
