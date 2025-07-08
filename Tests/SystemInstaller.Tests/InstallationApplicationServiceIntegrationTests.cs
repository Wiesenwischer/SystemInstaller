using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemInstaller.Application.UseCases;
using SystemInstaller.Application.DTOs;
using SystemInstaller.Infrastructure.Data;
using SystemInstaller.Infrastructure.Repositories;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.ValueObjects;
using SystemInstaller.Domain.Enums;
using SystemInstaller.Web.Services;
using Xunit;

namespace SystemInstaller.Tests;

public class InstallationApplicationServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly SystemInstallerDbContext _context;
    private readonly InstallationApplicationService _installationService;

    public InstallationApplicationServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<SystemInstallerDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Add repositories
        services.AddScoped<IInstallationEnvironmentRepository, InstallationEnvironmentRepository>();
        services.AddScoped<IInstallationTaskRepository, InstallationTaskRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        
        // Add other services
        services.AddHttpClient(); // For AgentApiClient
        services.AddScoped<AgentApiClient>();
        services.AddLogging();
        
        // Add application service
        services.AddScoped<InstallationApplicationService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<SystemInstallerDbContext>();
        _installationService = _serviceProvider.GetRequiredService<InstallationApplicationService>();
    }

    [Fact]
    public async Task Can_Create_And_Retrieve_Environment_And_Task()
    {
        // Setup: create tenant
        var tenant = new Tenant("InstallTenant", new Email("install@example.com"), "desc");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // Act: create environment
        var envDto = new CreateInstallationEnvironmentDto {
            TenantId = tenant.Id,
            Name = "Env1",
            Description = "desc"
        };
        var environmentResult = await _installationService.CreateEnvironmentAsync(envDto);
        Assert.NotNull(environmentResult);
        Assert.Equal("Env1", environmentResult.Name);

        // Act: create task
        var taskDto = new CreateInstallationTaskDto {
            EnvironmentId = environmentResult.Id,
            Name = "Task1",
            Description = "desc"
        };
        var taskResult = await _installationService.CreateTaskAsync(taskDto);
        Assert.NotNull(taskResult);
        Assert.Equal("Task1", taskResult.Name);
        Assert.Equal(InstallationStatus.Pending, taskResult.Status);

        // Retrieve all tasks for environment
        var allTasks = await _installationService.GetAllTasksAsync(environmentResult.Id);
        Assert.Contains(allTasks, t => t.Name == "Task1");
    }

    [Fact]
    public async Task Can_Start_And_Cancel_Installation_Task()
    {
        // Setup: create tenant, environment, and task
        var tenant = new Tenant("StartTestTenant", new Email("start@example.com"), "desc");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var envDto = new CreateInstallationEnvironmentDto {
            TenantId = tenant.Id,
            Name = "StartEnv",
            Description = "desc"
        };
        var environment = await _installationService.CreateEnvironmentAsync(envDto);

        var taskDto = new CreateInstallationTaskDto {
            EnvironmentId = environment.Id,
            Name = "StartTask",
            Description = "desc"
        };
        var task = await _installationService.CreateTaskAsync(taskDto);

        // Start the installation
        var startResult = await _installationService.StartInstallationAsync(task.Id);
        Assert.True(startResult);

        // Verify task is running
        var runningTask = await _installationService.GetTaskAsync(task.Id);
        Assert.NotNull(runningTask);
        Assert.Equal(InstallationStatus.Running, runningTask.Status);

        // Cancel the installation
        var cancelResult = await _installationService.CancelInstallationAsync(task.Id);
        Assert.True(cancelResult);

        // Verify task is cancelled
        var cancelledTask = await _installationService.GetTaskAsync(task.Id);
        Assert.NotNull(cancelledTask);
        Assert.Equal(InstallationStatus.Cancelled, cancelledTask.Status);
    }

    [Fact]
    public async Task Can_List_Environments_For_Tenant()
    {
        // Setup: create tenant
        var tenant = new Tenant("ListTestTenant", new Email("list@example.com"), "desc");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // Create multiple environments
        var env1Dto = new CreateInstallationEnvironmentDto {
            TenantId = tenant.Id,
            Name = "Env1",
            Description = "First environment"
        };
        var env1 = await _installationService.CreateEnvironmentAsync(env1Dto);

        var env2Dto = new CreateInstallationEnvironmentDto {
            TenantId = tenant.Id,
            Name = "Env2",
            Description = "Second environment"
        };
        var env2 = await _installationService.CreateEnvironmentAsync(env2Dto);

        // List environments for tenant
        var environments = await _installationService.GetEnvironmentsAsync(tenant.Id);
        Assert.Equal(2, environments.Count);
        Assert.Contains(environments, e => e.Name == "Env1");
        Assert.Contains(environments, e => e.Name == "Env2");
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
