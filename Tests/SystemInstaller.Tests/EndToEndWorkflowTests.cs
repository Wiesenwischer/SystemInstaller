using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemInstaller.Application.UseCases;
using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Infrastructure.Data;
using SystemInstaller.Infrastructure.Repositories;
using SystemInstaller.Infrastructure.Services;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.Services;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.ValueObjects;
using SystemInstaller.Domain.Enums;
using SystemInstaller.Web.Services;
using Xunit;

namespace SystemInstaller.Tests;

public class EndToEndWorkflowTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly SystemInstallerDbContext _context;
    private readonly TenantApplicationService _tenantService;
    private readonly InstallationApplicationService _installationService;
    private readonly UserInvitationApplicationService _invitationService;

    public EndToEndWorkflowTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<SystemInstallerDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Add repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantUserRepository, TenantUserRepository>();
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IInstallationEnvironmentRepository, InstallationEnvironmentRepository>();
        services.AddScoped<IInstallationTaskRepository, InstallationTaskRepository>();
        
        // Add domain services
        services.AddScoped<ITenantDomainService, TenantDomainService>();
        
        // Add infrastructure services
        services.AddScoped<IEmailService, EmailService>();
        
        // Add other services
        services.AddHttpClient(); // For AgentApiClient
        services.AddScoped<AgentApiClient>();
        services.AddLogging();
        
        // Add application services
        services.AddScoped<TenantApplicationService>();
        services.AddScoped<InstallationApplicationService>();
        services.AddScoped<UserInvitationApplicationService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<SystemInstallerDbContext>();
        _tenantService = _serviceProvider.GetRequiredService<TenantApplicationService>();
        _installationService = _serviceProvider.GetRequiredService<InstallationApplicationService>();
        _invitationService = _serviceProvider.GetRequiredService<UserInvitationApplicationService>();
    }

    [Fact]
    public async Task Complete_Tenant_Environment_Installation_Workflow()
    {
        // Step 1: Create a new tenant
        var createTenantDto = new CreateTenantDto(
            "E2E Test Company",
            "admin@e2etest.com",
            "End-to-end test tenant"
        );
        var tenant = await _tenantService.CreateTenantAsync(createTenantDto);
        Assert.NotNull(tenant);
        Assert.Equal("E2E Test Company", tenant.Name);

        // Step 2: Create environments for the tenant
        var devEnvDto = new CreateInstallationEnvironmentDto {
            TenantId = tenant.Id,
            Name = "Development",
            Description = "Development environment"
        };
        var devEnvironment = await _installationService.CreateEnvironmentAsync(devEnvDto);
        Assert.Equal("Development", devEnvironment.Name);

        var prodEnvDto = new CreateInstallationEnvironmentDto {
            TenantId = tenant.Id,
            Name = "Production",
            Description = "Production environment"
        };
        var prodEnvironment = await _installationService.CreateEnvironmentAsync(prodEnvDto);
        Assert.Equal("Production", prodEnvironment.Name);

        // Step 3: Create installation tasks
        var webAppTaskDto = new CreateInstallationTaskDto {
            EnvironmentId = devEnvironment.Id,
            Name = "Deploy Web Application",
            Description = "Deploy the main web application"
        };
        var webAppTask = await _installationService.CreateTaskAsync(webAppTaskDto);
        Assert.Equal(InstallationStatus.Pending, webAppTask.Status);

        var dbTaskDto = new CreateInstallationTaskDto {
            EnvironmentId = devEnvironment.Id,
            Name = "Setup Database",
            Description = "Initialize and configure the database"
        };
        var dbTask = await _installationService.CreateTaskAsync(dbTaskDto);
        Assert.Equal(InstallationStatus.Pending, dbTask.Status);

        // Step 4: Start installation process
        var startWebAppResult = await _installationService.StartInstallationAsync(webAppTask.Id);
        Assert.True(startWebAppResult);

        // Verify task is running
        var runningWebAppTask = await _installationService.GetTaskAsync(webAppTask.Id);
        Assert.NotNull(runningWebAppTask);
        Assert.Equal(InstallationStatus.Running, runningWebAppTask.Status);

        // Step 5: Verify all data is correctly stored and retrievable
        var retrievedTenant = await _tenantService.GetTenantByIdAsync(tenant.Id);
        Assert.NotNull(retrievedTenant);
        Assert.Equal("E2E Test Company", retrievedTenant.Name);

        var environments = await _installationService.GetEnvironmentsAsync(tenant.Id);
        Assert.Equal(2, environments.Count);
        Assert.Contains(environments, e => e.Name == "Development");
        Assert.Contains(environments, e => e.Name == "Production");

        var devTasks = await _installationService.GetAllTasksAsync(devEnvironment.Id);
        Assert.Equal(2, devTasks.Count);
        Assert.Contains(devTasks, t => t.Name == "Deploy Web Application");
        Assert.Contains(devTasks, t => t.Name == "Setup Database");
    }

    [Fact]
    public async Task Complete_User_Invitation_Workflow()
    {
        // Step 1: Create a tenant
        var createTenantDto = new CreateTenantDto(
            "Invitation Test Company",
            "admin@invtest.com",
            "Invitation test tenant"
        );
        var tenant = await _tenantService.CreateTenantAsync(createTenantDto);

        // Step 2: Create user invitation
        var invitationDto = new CreateUserInvitationDto {
            TenantId = tenant.Id,
            Email = "newuser@invtest.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Developer",
            InvitedByUserId = "admin-user-123"
        };
        var invitation = await _invitationService.CreateInvitationAsync(invitationDto);
        Assert.NotNull(invitation);
        Assert.Equal("newuser@invtest.com", invitation.Email);
        Assert.False(invitation.IsUsed);

        // Step 3: Retrieve invitation by token
        var retrievedInvitation = await _invitationService.GetInvitationByTokenAsync(invitation.InvitationToken);
        Assert.NotNull(retrievedInvitation);
        Assert.Equal(invitation.Id, retrievedInvitation.Id);

        // Step 4: Accept invitation
        var acceptDto = new AcceptInvitationDto(invitation.InvitationToken, "new-user-123");
        var acceptResult = await _invitationService.AcceptInvitationAsync(acceptDto);
        Assert.True(acceptResult);

        // Step 5: Verify invitation is used
        var usedInvitation = await _invitationService.GetInvitationByTokenAsync(invitation.InvitationToken);
        Assert.NotNull(usedInvitation);
        Assert.True(usedInvitation.IsUsed);
        Assert.NotNull(usedInvitation.UsedAt);
    }

    [Fact]
    public async Task Tenant_Lifecycle_Management()
    {
        // Create tenant
        var createDto = new CreateTenantDto(
            "Lifecycle Test Tenant",
            "lifecycle@test.com",
            "Testing tenant lifecycle"
        );
        var tenant = await _tenantService.CreateTenantAsync(createDto);
        Assert.True(tenant.IsActive);

        // Update tenant
        var updateDto = new UpdateTenantDto(
            "Updated Lifecycle Tenant",
            "Updated description",
            "updated@test.com"
        );
        var updatedTenant = await _tenantService.UpdateTenantAsync(tenant.Id, updateDto);
        Assert.Equal("Updated Lifecycle Tenant", updatedTenant.Name);

        // Deactivate tenant
        var deactivatedTenant = await _tenantService.DeactivateTenantAsync(tenant.Id);
        Assert.False(deactivatedTenant.IsActive);

        // Reactivate tenant
        var reactivatedTenant = await _tenantService.ActivateTenantAsync(tenant.Id);
        Assert.True(reactivatedTenant.IsActive);

        // Get tenant details
        var tenantDetails = await _tenantService.GetTenantDetailsAsync(tenant.Id);
        Assert.NotNull(tenantDetails);
        Assert.Equal("Updated Lifecycle Tenant", tenantDetails.Name);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
