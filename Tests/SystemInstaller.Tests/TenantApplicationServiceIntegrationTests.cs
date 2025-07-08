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
using SystemInstaller.Domain.Services;
using SystemInstaller.Domain.Entities;
using Xunit;

namespace SystemInstaller.Tests;

public class TenantApplicationServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly SystemInstallerDbContext _context;
    private readonly TenantApplicationService _tenantService;

    public TenantApplicationServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<SystemInstallerDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Add repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantUserRepository, TenantUserRepository>();
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IInstallationEnvironmentRepository, InstallationEnvironmentRepository>();
        
        // Add domain services
        services.AddScoped<ITenantDomainService, TenantDomainService>();
        
        // Add logging
        services.AddLogging();
        
        // Add application service
        services.AddScoped<TenantApplicationService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<SystemInstallerDbContext>();
        _tenantService = _serviceProvider.GetRequiredService<TenantApplicationService>();
    }

    [Fact]
    public async Task Can_Create_And_Retrieve_Tenant()
    {
        var tenantDto = new CreateTenantDto(
            "Integration Tenant",
            "integration@example.com",
            "Integration test tenant"
        );
        var createdTenant = await _tenantService.CreateTenantAsync(tenantDto);
        Assert.NotNull(createdTenant);
        var fetched = await _tenantService.GetTenantByIdAsync(createdTenant.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Integration Tenant", fetched.Name);
        Assert.Equal("integration@example.com", fetched.ContactEmail);
    }

    [Fact]
    public async Task Can_Update_Tenant()
    {
        // Create tenant
        var createDto = new CreateTenantDto(
            "Update Test Tenant",
            "update@example.com",
            "Original description"
        );
        var createdTenant = await _tenantService.CreateTenantAsync(createDto);
        
        // Update tenant
        var updateDto = new UpdateTenantDto(
            "Updated Tenant Name",
            "Updated description",
            "updated@example.com"
        );
        var updatedTenant = await _tenantService.UpdateTenantAsync(createdTenant.Id, updateDto);
        
        Assert.Equal("Updated Tenant Name", updatedTenant.Name);
        Assert.Equal("updated@example.com", updatedTenant.ContactEmail);
        Assert.Equal("Updated description", updatedTenant.Description);
    }

    [Fact]
    public async Task Can_Activate_And_Deactivate_Tenant()
    {
        // Create tenant
        var createDto = new CreateTenantDto("Status Test", "status@example.com", null);
        var tenant = await _tenantService.CreateTenantAsync(createDto);
        Assert.True(tenant.IsActive);
        
        // Deactivate
        var deactivatedTenant = await _tenantService.DeactivateTenantAsync(tenant.Id);
        Assert.False(deactivatedTenant.IsActive);
        
        // Activate
        var activatedTenant = await _tenantService.ActivateTenantAsync(tenant.Id);
        Assert.True(activatedTenant.IsActive);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
