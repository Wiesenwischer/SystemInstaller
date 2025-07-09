using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.IntegrationTests.Mocks;
using SystemInstaller.Web.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace SystemInstaller.IntegrationTests.ComponentTests;

public class TenantDetailsComponentTests : BlazorComponentTestBase
{
    public TenantDetailsComponentTests()
    {
        // Setup authentication
        var mockUser = MockUsers.CreateAuthenticatedUser();
        var authStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
    }

    [Fact]
    public async Task TenantDetails_ShouldDisplayTenantInformation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Test Company", "A comprehensive test company");
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        Assert.Contains("Test Company", component.Markup);
        Assert.Contains("A comprehensive test company", component.Markup);
    }

    [Fact]
    public async Task TenantDetails_ShouldDisplayEnvironments()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environments = TestDataFactory.CreateMultipleEnvironments(tenant.Id, 3);

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddRangeAsync(environments);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        Assert.Contains("Environments", component.Markup);
        
        foreach (var env in environments)
        {
            Assert.Contains(env.Name, component.Markup);
        }
    }

    [Fact]
    public async Task TenantDetails_ShouldDisplayTenantUsers()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var user1 = TestDataFactory.CreateUser("user1@test.com", "User One");
        var user2 = TestDataFactory.CreateUser("user2@test.com", "User Two");
        
        var tenantUser1 = TestDataFactory.CreateTenantUser(tenant.Id, user1.Id, "Admin");
        var tenantUser2 = TestDataFactory.CreateTenantUser(tenant.Id, user2.Id, "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddRangeAsync(user1, user2);
        await DbContext.TenantUsers.AddRangeAsync(tenantUser1, tenantUser2);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        Assert.Contains("Users", component.Markup);
        Assert.Contains("User One", component.Markup);
        Assert.Contains("User Two", component.Markup);
        Assert.Contains("Admin", component.Markup);
        Assert.Contains("Member", component.Markup);
    }

    [Fact]
    public async Task TenantDetails_ShouldDisplayPendingInvitations()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation1 = TestDataFactory.CreateUserInvitation(tenant.Id, "invite1@test.com");
        var invitation2 = TestDataFactory.CreateUserInvitation(tenant.Id, "invite2@test.com");
        
        invitation1.Status = Domain.Enums.InvitationStatus.Pending;
        invitation2.Status = Domain.Enums.InvitationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddRangeAsync(invitation1, invitation2);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        Assert.Contains("Pending Invitations", component.Markup);
        Assert.Contains("invite1@test.com", component.Markup);
        Assert.Contains("invite2@test.com", component.Markup);
    }

    [Fact]
    public async Task TenantDetails_ShouldHandleInvalidTenantId()
    {
        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, Guid.NewGuid().ToString()));

        // Assert
        Assert.Contains("Tenant not found", component.Markup);
    }

    [Fact]
    public async Task TenantDetails_ShouldDisplayInviteUserButton()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        var inviteButton = component.Find("button:contains('Invite User')");
        Assert.NotNull(inviteButton);
    }

    [Fact]
    public async Task TenantDetails_ShouldDisplayRemoveUserButtons()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var user = TestDataFactory.CreateUser("removable@test.com", "Removable User");
        var tenantUser = TestDataFactory.CreateTenantUser(tenant.Id, user.Id, "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddAsync(user);
        await DbContext.TenantUsers.AddAsync(tenantUser);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        var removeButtons = component.FindAll("button:contains('Remove')");
        Assert.NotEmpty(removeButtons);
    }

    [Fact]
    public async Task TenantDetails_ShouldShowUserRoleDropdown()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var user = TestDataFactory.CreateUser("editable@test.com", "Editable User");
        var tenantUser = TestDataFactory.CreateTenantUser(tenant.Id, user.Id, "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddAsync(user);
        await DbContext.TenantUsers.AddAsync(tenantUser);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        var roleSelects = component.FindAll("select");
        Assert.NotEmpty(roleSelects);
    }

    [Fact]
    public async Task TenantDetails_ShouldDisplayEnvironmentCreationLink()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        var createEnvLink = component.Find($"a[href='/environments/new?tenantId={tenant.Id}']");
        Assert.NotNull(createEnvLink);
    }

    [Fact]
    public async Task TenantDetails_ShouldShowEnvironmentDetails()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var environment = TestDataFactory.CreateEnvironment(tenant.Id, "Production Environment");
        environment.ServerUrl = "https://prod.example.com";

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.InstallationEnvironments.AddAsync(environment);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<TenantDetails>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        Assert.Contains("Production Environment", component.Markup);
        Assert.Contains("https://prod.example.com", component.Markup);
    }
}
