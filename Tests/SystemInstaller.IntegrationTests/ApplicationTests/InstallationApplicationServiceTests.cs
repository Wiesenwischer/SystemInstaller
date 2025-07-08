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
    public async Task GetAllInstallationsAsync_ShouldReturnAllInstallations()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        var installation1 = TestDataFactory.CreateInstallation(environment.Id, "1.0.0");
        var installation2 = TestDataFactory.CreateInstallation(environment.Id, "2.0.0");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddRangeAsync(installation1, installation2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetAllInstallationsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Version == "1.0.0");
        Assert.Contains(result, i => i.Version == "2.0.0");
    }

    [Fact]
    public async Task GetInstallationByIdAsync_ShouldReturnCorrectInstallation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id, "Test Environment");
        var installation = TestDataFactory.CreateInstallation(environment.Id, "3.1.0");
        installation.Status = InstallationStatus.Running;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetInstallationByIdAsync(installation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(installation.Id, result.Id);
        Assert.Equal("3.1.0", result.Version);
        Assert.Equal("Running", result.Status);
        Assert.Equal("Test Environment", result.EnvironmentName);
    }

    [Fact]
    public async Task GetInstallationByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _installationService.GetInstallationByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateInstallationAsync_ShouldCreateNewInstallation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateInstallationDto
        {
            EnvironmentId = environment.Id,
            Version = "4.0.0"
        };

        // Act
        var result = await _installationService.CreateInstallationAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("4.0.0", result.Version);
        Assert.Equal("Pending", result.Status);

        var dbInstallation = await DbContext.Installations.FindAsync(result.Id);
        Assert.NotNull(dbInstallation);
        Assert.Equal(environment.Id, dbInstallation.EnvironmentId);
    }

    [Fact]
    public async Task StartInstallationAsync_ShouldChangeStatusToRunning()
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
        await _installationService.StartInstallationAsync(installation.Id);

        // Assert
        var dbInstallation = await DbContext.Installations.FindAsync(installation.Id);
        Assert.Equal(InstallationStatus.Running, dbInstallation.Status);
        Assert.NotNull(dbInstallation.StartedAt);
    }

    [Fact]
    public async Task CancelInstallationAsync_ShouldChangeStatusToCancelled()
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
        await _installationService.CancelInstallationAsync(installation.Id);

        // Assert
        var dbInstallation = await DbContext.Installations.FindAsync(installation.Id);
        Assert.Equal(InstallationStatus.Cancelled, dbInstallation.Status);
    }

    [Fact]
    public async Task GetInstallationDetailsAsync_ShouldReturnWithTasks()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Detail Tenant");
        var environment = TestDataFactory.CreateEnvironment(tenant.Id, "Detail Environment");
        var installation = TestDataFactory.CreateInstallation(environment.Id, "5.0.0");
        
        var task1 = TestDataFactory.CreateInstallationTask(installation.Id, "Task 1");
        task1.Status = InstallationStatus.Completed;
        task1.Order = 1;
        
        var task2 = TestDataFactory.CreateInstallationTask(installation.Id, "Task 2");
        task2.Status = InstallationStatus.Running;
        task2.Order = 2;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddAsync(installation);
        await DbContext.InstallationTasks.AddRangeAsync(task1, task2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetInstallationDetailsAsync(installation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("5.0.0", result.Version);
        Assert.Equal("Detail Environment", result.EnvironmentName);
        Assert.Equal("Detail Tenant", result.TenantName);
        Assert.Equal(2, result.Tasks.Count());
        
        var orderedTasks = result.Tasks.OrderBy(t => t.Order).ToList();
        Assert.Equal("Task 1", orderedTasks[0].Name);
        Assert.Equal("Completed", orderedTasks[0].Status);
        Assert.Equal("Task 2", orderedTasks[1].Name);
        Assert.Equal("Running", orderedTasks[1].Status);
    }

    [Fact]
    public async Task GetActiveEnvironmentsAsync_ShouldReturnOnlyActiveEnvironments()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var activeEnv = TestDataFactory.CreateEnvironment(tenant.Id, "Active");
        activeEnv.IsActive = true;
        
        var inactiveEnv = TestDataFactory.CreateEnvironment(tenant.Id, "Inactive");
        inactiveEnv.IsActive = false;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddRangeAsync(activeEnv, inactiveEnv);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetActiveEnvironmentsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result.First().Name);
    }

    [Fact]
    public async Task GetInstallationStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        
        var pendingInstallation = TestDataFactory.CreateInstallation(environment.Id);
        pendingInstallation.Status = InstallationStatus.Pending;
        
        var runningInstallation = TestDataFactory.CreateInstallation(environment.Id);
        runningInstallation.Status = InstallationStatus.Running;
        
        var completedInstallation1 = TestDataFactory.CreateInstallation(environment.Id);
        completedInstallation1.Status = InstallationStatus.Completed;
        
        var completedInstallation2 = TestDataFactory.CreateInstallation(environment.Id);
        completedInstallation2.Status = InstallationStatus.Completed;
        
        var failedInstallation = TestDataFactory.CreateInstallation(environment.Id);
        failedInstallation.Status = InstallationStatus.Failed;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddRangeAsync(
            pendingInstallation, runningInstallation, 
            completedInstallation1, completedInstallation2, 
            failedInstallation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetInstallationStatisticsAsync();

        // Assert
        Assert.Equal(5, result.TotalInstallations);
        Assert.Equal(1, result.PendingInstallations);
        Assert.Equal(1, result.RunningInstallations);
        Assert.Equal(2, result.CompletedInstallations);
        Assert.Equal(1, result.FailedInstallations);
    }

    [Fact]
    public async Task GetRecentInstallationsAsync_ShouldReturnMostRecent()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id);
        
        var oldInstallation = TestDataFactory.CreateInstallation(environment.Id, "1.0.0");
        oldInstallation.CreatedAt = DateTime.UtcNow.AddDays(-10);
        
        var recentInstallation = TestDataFactory.CreateInstallation(environment.Id, "2.0.0");
        recentInstallation.CreatedAt = DateTime.UtcNow.AddHours(-1);

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.Installations.AddRangeAsync(oldInstallation, recentInstallation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _installationService.GetRecentInstallationsAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("2.0.0", result.First().Version);
    }

    [Fact]
    public async Task CreateEnvironmentAsync_ShouldCreateNewEnvironment()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateEnvironmentDto
        {
            Name = "New Environment",
            Description = "New Description",
            ServerUrl = "https://new.example.com",
            TenantId = tenant.Id
        };

        // Act
        var result = await _installationService.CreateEnvironmentAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Environment", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.Equal("https://new.example.com", result.ServerUrl);
        Assert.True(result.IsActive);

        var dbEnvironment = await DbContext.InstallationEnvironments.FindAsync(result.Id);
        Assert.NotNull(dbEnvironment);
        Assert.Equal(tenant.Id, dbEnvironment.TenantId);
    }

    [Fact]
    public async Task RetryInstallationAsync_ShouldResetFailedInstallation()
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
        await _installationService.RetryInstallationAsync(installation.Id);

        // Assert
        var dbInstallation = await DbContext.Installations.FindAsync(installation.Id);
        Assert.Equal(InstallationStatus.Pending, dbInstallation.Status);
        Assert.Null(dbInstallation.StartedAt);
        Assert.Null(dbInstallation.CompletedAt);
    }
}
