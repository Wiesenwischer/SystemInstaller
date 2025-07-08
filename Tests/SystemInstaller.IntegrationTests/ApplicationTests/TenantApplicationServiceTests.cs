using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Application.DTOs;

namespace SystemInstaller.IntegrationTests.ApplicationTests;

public class TenantApplicationServiceTests : BlazorComponentTestBase
{
    private readonly ITenantApplicationService _tenantService;

    public TenantApplicationServiceTests()
    {
        _tenantService = WebApplicationFactory.Services.GetRequiredService<ITenantApplicationService>();
    }

    [Fact]
    public async Task GetAllTenantsAsync_ShouldReturnAllActiveTenants()
    {
        // Arrange
        var activeTenant1 = TestDataFactory.CreateTenant("Active Tenant 1");
        activeTenant1.IsActive = true;
        
        var activeTenant2 = TestDataFactory.CreateTenant("Active Tenant 2");
        activeTenant2.IsActive = true;
        
        var inactiveTenant = TestDataFactory.CreateTenant("Inactive Tenant");
        inactiveTenant.IsActive = false;

        await DbContext.Tenants.AddRangeAsync(activeTenant1, activeTenant2, inactiveTenant);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _tenantService.GetAllTenantsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Name == "Active Tenant 1");
        Assert.Contains(result, t => t.Name == "Active Tenant 2");
        Assert.DoesNotContain(result, t => t.Name == "Inactive Tenant");
    }

    [Fact]
    public async Task GetTenantByIdAsync_ShouldReturnCorrectTenant()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Test Tenant", "Test Description");
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _tenantService.GetTenantByIdAsync(tenant.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenant.Id, result.Id);
        Assert.Equal("Test Tenant", result.Name);
        Assert.Equal("Test Description", result.Description);
    }

    [Fact]
    public async Task GetTenantByIdAsync_ShouldReturnNull_WhenTenantNotFound()
    {
        // Act
        var result = await _tenantService.GetTenantByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTenantAsync_ShouldCreateNewTenant()
    {
        // Arrange
        var createDto = new CreateTenantDto
        {
            Name = "New Tenant",
            Description = "New Description"
        };

        // Act
        var result = await _tenantService.CreateTenantAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Tenant", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.True(result.IsActive);

        var dbTenant = await DbContext.Tenants.FindAsync(result.Id);
        Assert.NotNull(dbTenant);
        Assert.Equal("New Tenant", dbTenant.Name);
    }

    [Fact]
    public async Task UpdateTenantAsync_ShouldUpdateExistingTenant()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Original Name", "Original Description");
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        var updateDto = new UpdateTenantDto
        {
            Id = tenant.Id,
            Name = "Updated Name",
            Description = "Updated Description"
        };

        // Act
        var result = await _tenantService.UpdateTenantAsync(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Description", result.Description);

        var dbTenant = await DbContext.Tenants.FindAsync(tenant.Id);
        Assert.Equal("Updated Name", dbTenant.Name);
        Assert.Equal("Updated Description", dbTenant.Description);
    }

    [Fact]
    public async Task DeleteTenantAsync_ShouldDeactivateTenant()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        tenant.IsActive = true;
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        await _tenantService.DeleteTenantAsync(tenant.Id);

        // Assert
        var dbTenant = await DbContext.Tenants.FindAsync(tenant.Id);
        Assert.NotNull(dbTenant);
        Assert.False(dbTenant.IsActive);
    }

    [Fact]
    public async Task GetTenantDetailsAsync_ShouldReturnCompleteDetails()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Detailed Tenant");
        var user1 = TestDataFactory.CreateUser("user1@test.com", "User One");
        var user2 = TestDataFactory.CreateUser("user2@test.com", "User Two");
        var environment = TestDataFactory.CreateEnvironment(tenant.Id, "Test Environment");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "invite@test.com");
        
        var tenantUser1 = TestDataFactory.CreateTenantUser(tenant.Id, user1.Id, "Admin");
        var tenantUser2 = TestDataFactory.CreateTenantUser(tenant.Id, user2.Id, "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddRangeAsync(user1, user2);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.TenantUsers.AddRangeAsync(tenantUser1, tenantUser2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _tenantService.GetTenantDetailsAsync(tenant.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Detailed Tenant", result.Name);
        Assert.Equal(2, result.Users.Count());
        Assert.Equal(1, result.Environments.Count());
        Assert.Equal(1, result.PendingInvitations.Count());
        
        Assert.Contains(result.Users, u => u.Email == "user1@test.com" && u.Role == "Admin");
        Assert.Contains(result.Users, u => u.Email == "user2@test.com" && u.Role == "Member");
        Assert.Contains(result.Environments, e => e.Name == "Test Environment");
        Assert.Contains(result.PendingInvitations, i => i.Email == "invite@test.com");
    }

    [Fact]
    public async Task RemoveUserFromTenantAsync_ShouldRemoveUserAssociation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var user = TestDataFactory.CreateUser("remove@test.com", "Remove User");
        var tenantUser = TestDataFactory.CreateTenantUser(tenant.Id, user.Id, "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddAsync(user);
        await DbContext.TenantUsers.AddAsync(tenantUser);
        await DbContext.SaveChangesAsync();

        // Act
        await _tenantService.RemoveUserFromTenantAsync(tenant.Id, user.Id);

        // Assert
        var removedTenantUser = DbContext.TenantUsers
            .FirstOrDefault(tu => tu.TenantId == tenant.Id && tu.UserId == user.Id);
        Assert.Null(removedTenantUser);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldUpdateUserRole()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var user = TestDataFactory.CreateUser("role@test.com", "Role User");
        var tenantUser = TestDataFactory.CreateTenantUser(tenant.Id, user.Id, "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddAsync(user);
        await DbContext.TenantUsers.AddAsync(tenantUser);
        await DbContext.SaveChangesAsync();

        // Act
        await _tenantService.UpdateUserRoleAsync(tenant.Id, user.Id, "Admin");

        // Assert
        var updatedTenantUser = DbContext.TenantUsers
            .First(tu => tu.TenantId == tenant.Id && tu.UserId == user.Id);
        Assert.Equal("Admin", updatedTenantUser.Role);
    }

    [Fact]
    public async Task CreateTenantAsync_ShouldThrowException_WhenDuplicateName()
    {
        // Arrange
        var existingTenant = TestDataFactory.CreateTenant("Duplicate Name");
        await DbContext.Tenants.AddAsync(existingTenant);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateTenantDto
        {
            Name = "Duplicate Name",
            Description = "Different Description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _tenantService.CreateTenantAsync(createDto));
    }

    [Fact]
    public async Task GetTenantDetailsAsync_ShouldReturnNull_WhenTenantNotFound()
    {
        // Act
        var result = await _tenantService.GetTenantDetailsAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
