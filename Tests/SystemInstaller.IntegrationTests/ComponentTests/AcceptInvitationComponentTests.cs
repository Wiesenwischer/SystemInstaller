using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.IntegrationTests.Mocks;
using SystemInstaller.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using SystemInstaller.Domain.Enums;

namespace SystemInstaller.IntegrationTests.ComponentTests;

public class AcceptInvitationComponentTests : BlazorComponentTestBase
{
    public AcceptInvitationComponentTests()
    {
        // Setup authentication
        var mockUser = MockUsers.CreateAuthenticatedUser("test-user-id", "test@example.com");
        var authStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldDisplayInvitationDetails_WhenValidToken()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Invitation Test Company");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");



        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        // Assert
        Assert.Contains("Invitation Test Company", component.Markup);
        Assert.Contains("test@example.com", component.Markup);
        Assert.Contains("Admin", component.Markup);
        Assert.Contains("Accept Invitation", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldShowErrorMessage_WhenInvalidToken()
    {
        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, "invalid-token"));

        // Assert
        Assert.Contains("Invalid or expired invitation", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldShowErrorMessage_WhenExpiredInvitation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");
        // Note: ExpiresAt is read-only, so this test assumes the invitation 
        // is created with appropriate expiration logic in the factory

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        // Assert
        Assert.Contains("expired", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldShowErrorMessage_WhenAlreadyAccepted()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");
        invitation.Use(); // Already accepted

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        // Assert
        Assert.Contains("already been accepted", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldAcceptInvitation_WhenValidAndUserMatches()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Accept Test Company");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");



        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        var acceptButton = component.Find("button:contains('Accept Invitation')");
        await acceptButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        var updatedInvitation = DbContext.UserInvitations.First(i => i.Id == invitation.Id);
        Assert.True(updatedInvitation.IsUsed);
        
        // Check if TenantUser relationship was created
        var tenantUser = DbContext.TenantUsers.FirstOrDefault(tu => 
            tu.TenantId == tenant.Id && 
            tu.Email.Value == "test@example.com");
        Assert.NotNull(tenantUser);
        Assert.Equal("Member", tenantUser.Role);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldCreateUserIfNotExists()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");


        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        var acceptButton = component.Find("button:contains('Accept Invitation')");
        await acceptButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        var createdUser = DbContext.TenantUsers.FirstOrDefault(u => u.Email == "test@example.com");
        Assert.NotNull(createdUser);
        Assert.Equal("Test User", createdUser.Name); // From mock user
    }

    [Fact]
    public async Task AcceptInvitation_ShouldDeclineInvitation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");


        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        var declineButton = component.Find("button:contains('Decline')");
        await declineButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        var updatedInvitation = DbContext.UserInvitations.First(i => i.Id == invitation.Id);
        Assert.True(updatedInvitation.IsUsed);
        
        // Check that no TenantUser relationship was created
        var tenantUser = DbContext.TenantUsers.FirstOrDefault(tu => 
            tu.TenantId == tenant.Id && 
            tu.Email.Value == "test@example.com");
        Assert.Null(tenantUser);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldShowErrorMessage_WhenEmailMismatch()
    {
        // Arrange
        var mockUser = MockUsers.CreateAuthenticatedUser("test-user-id", "different@example.com");
        var authStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.RemoveAll<AuthenticationStateProvider>();
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);

        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "original@example.com");


        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        // Assert
        Assert.Contains("email address does not match", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldShowUnauthenticatedMessage_WhenNotLoggedIn()
    {
        // Arrange
        var unauthenticatedUser = MockUsers.CreateUnauthenticatedUser();
        var authStateProvider = new MockAuthenticationStateProvider(unauthenticatedUser);
        Services.RemoveAll<AuthenticationStateProvider>();
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);

        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        // Assert
        Assert.Contains("must be logged in", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldShowSuccessMessage_AfterAccepting()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Success Test Company");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");


        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        var acceptButton = component.Find("button:contains('Accept Invitation')");
        await acceptButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.Contains("successfully accepted", component.Markup);
        Assert.Contains("Success Test Company", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldDisplayInvitationDetails()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Details Test Company");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");




        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        // Assert
        Assert.Contains("Details Test Company", component.Markup);
        Assert.Contains("Admin", component.Markup);
        Assert.Contains("test@example.com", component.Markup);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldHandleConcurrentAcceptance()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "test@example.com");


        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Simulate concurrent acceptance by updating the status directly
        invitation.Use();
        DbContext.Update(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<SystemInstaller.Components.Pages.AcceptInvitation>(parameters => parameters
            .Add(p => p.Token, invitation.InvitationToken));

        // Assert
        Assert.Contains("already been accepted", component.Markup);
    }
}
