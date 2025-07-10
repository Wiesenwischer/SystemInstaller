using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.IntegrationTests.Mocks;
using SystemInstaller.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using AngleSharp.Dom;

namespace SystemInstaller.IntegrationTests.ComponentTests;

public class TenantsComponentTests : BlazorComponentTestBase
{
    public TenantsComponentTests()
    {
        // Setup authentication
        var mockUser = MockUsers.CreateAuthenticatedUser();
        var authStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
    }

    [Fact]
    public async Task Tenants_ShouldDisplayTenantsWhenDataExists()
    {
        // Arrange
        var tenants = TestDataFactory.CreateMultipleTenants(3);
        await DbContext.Tenants.AddRangeAsync(tenants);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Tenants>();

        // Assert
        Assert.Contains("Tenants", component.Markup);
        
        foreach (var tenant in tenants)
        {
            Assert.Contains(tenant.Name, component.Markup);
            Assert.Contains(tenant.Description, component.Markup);
        }
    }

    [Fact]
    public async Task Tenants_ShouldDisplayEmptyStateWhenNoData()
    {
        // Act
        var component = RenderComponent<Tenants>();

        // Assert
        Assert.Contains("No tenants found", component.Markup);
    }

    [Fact]
    public async Task Tenants_ShouldNavigateToCreateTenant()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        // Act
        var component = RenderComponent<Tenants>();
        var createButton = component.Find("a[href='/tenants/new']");
        
        // Assert
        Assert.NotNull(createButton);
        Assert.Contains("Create New Tenant", createButton.TextContent);
    }

    [Fact]
    public async Task Tenants_ShouldDisplayTenantCards()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Test Company", "A test company description");
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Tenants>();

        // Assert
        var cards = component.FindAll(".card");
        Assert.Single(cards);
        
        var card = cards.First();
        Assert.Contains("Test Company", card.TextContent);
        Assert.Contains("A test company description", card.TextContent);
    }

    [Fact]
    public async Task Tenants_ShouldShowActiveTenants()
    {
        // Arrange
        var activeTenant = TestDataFactory.CreateTenant("Active Tenant", "Active description");
        activeTenant.Activate();
        
        var inactiveTenant = TestDataFactory.CreateTenant("Inactive Tenant", "Inactive description");
        inactiveTenant.Deactivate();

        await DbContext.Tenants.AddRangeAsync(activeTenant, inactiveTenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Tenants>();

        // Assert
        Assert.Contains("Active Tenant", component.Markup);
        Assert.DoesNotContain("Inactive Tenant", component.Markup);
    }

    [Fact]
    public async Task Tenants_ShouldHandleLoadingState()
    {
        // Act
        var component = RenderComponent<Tenants>();

        // Assert - Initial loading should be handled gracefully
        Assert.NotNull(component);
    }

    [Fact]
    public async Task Tenants_ShouldDisplayTenantDetailsLink()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Detail Test Tenant");
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<Tenants>();

        // Assert
        var detailsLink = component.Find($"a[href='/tenants/{tenant.Id}']");
        Assert.NotNull(detailsLink);
    }
}
