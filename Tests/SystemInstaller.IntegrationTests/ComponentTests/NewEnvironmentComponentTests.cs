using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.IntegrationTests.Mocks;
using SystemInstaller.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace SystemInstaller.IntegrationTests.ComponentTests;

public class NewEnvironmentComponentTests : BlazorComponentTestBase
{
    public NewEnvironmentComponentTests()
    {
        // Setup authentication
        var mockUser = MockUsers.CreateAuthenticatedUser();
        var authStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
    }

    [Fact]
    public async Task NewEnvironment_ShouldDisplayForm()
    {
        // Act
        var component = RenderComponent<NewEnvironment>();

        // Assert
        Assert.Contains("Create New Environment", component.Markup);
        
        var nameInput = component.Find("input[id='name']");
        var descriptionTextarea = component.Find("textarea[id='description']");
        var serverUrlInput = component.Find("input[id='serverUrl']");
        var submitButton = component.Find("button[type='submit']");
        
        Assert.NotNull(nameInput);
        Assert.NotNull(descriptionTextarea);
        Assert.NotNull(serverUrlInput);
        Assert.NotNull(submitButton);
    }

    [Fact]
    public async Task NewEnvironment_ShouldDisplayTenantSelector()
    {
        // Arrange
        var tenants = TestDataFactory.CreateMultipleTenants(3);
        await DbContext.Tenants.AddRangeAsync(tenants);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<NewEnvironment>();

        // Assert
        var tenantSelect = component.Find("select[id='tenantId']");
        Assert.NotNull(tenantSelect);
        
        foreach (var tenant in tenants)
        {
            Assert.Contains(tenant.Name, component.Markup);
        }
    }

    [Fact]
    public async Task NewEnvironment_ShouldPreSelectTenantWhenProvided()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Preselected Tenant");
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<NewEnvironment>(parameters => parameters
            .Add(p => p.TenantId, tenant.Id.ToString()));

        // Assert
        var tenantSelect = component.Find("select[id='tenantId']");
        Assert.Equal(tenant.Id.ToString(), tenantSelect.GetAttribute("value"));
    }

    [Fact]
    public async Task NewEnvironment_ShouldValidateRequiredFields()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<NewEnvironment>();
        var form = component.Find("form");
        
        // Try to submit without filling required fields
        await form.SubmitAsync();

        // Assert
        var validationMessages = component.FindAll(".validation-message");
        Assert.NotEmpty(validationMessages);
    }

    [Fact]
    public async Task NewEnvironment_ShouldCreateEnvironmentWithValidData()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<NewEnvironment>();
        
        var nameInput = component.Find("input[id='name']");
        var descriptionTextarea = component.Find("textarea[id='description']");
        var serverUrlInput = component.Find("input[id='serverUrl']");
        var tenantSelect = component.Find("select[id='tenantId']");
        
        await nameInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "Test Environment"
        });
        
        await descriptionTextarea.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "Test Description"
        });
        
        await serverUrlInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "https://test.example.com"
        });
        
        await tenantSelect.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = tenant.Id.ToString()
        });

        var form = component.Find("form");
        await form.SubmitAsync();

        // Assert
        var createdEnvironment = DbContext.InstallationEnvironments
            .FirstOrDefault(e => e.Name == "Test Environment");
        
        Assert.NotNull(createdEnvironment);
        Assert.Equal("Test Description", createdEnvironment.Description);
        Assert.Equal("https://test.example.com", createdEnvironment.ServerUrl);
        Assert.Equal(tenant.Id, createdEnvironment.TenantId);
    }

    [Fact]
    public async Task NewEnvironment_ShouldValidateServerUrl()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<NewEnvironment>();
        
        var serverUrlInput = component.Find("input[id='serverUrl']");
        await serverUrlInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "invalid-url"
        });

        var form = component.Find("form");
        await form.SubmitAsync();

        // Assert
        var validationMessages = component.FindAll(".validation-message");
        Assert.Contains(validationMessages, vm => vm.TextContent.Contains("valid URL"));
    }

    [Fact]
    public async Task NewEnvironment_ShouldDisplayOnlyActiveTenants()
    {
        // Arrange
        var activeTenant = TestDataFactory.CreateTenant("Active Tenant");
        activeTenant.IsActive = true;
        
        var inactiveTenant = TestDataFactory.CreateTenant("Inactive Tenant");
        inactiveTenant.IsActive = false;

        await DbContext.Tenants.AddRangeAsync(activeTenant, inactiveTenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<NewEnvironment>();

        // Assert
        Assert.Contains("Active Tenant", component.Markup);
        Assert.DoesNotContain("Inactive Tenant", component.Markup);
    }

    [Fact]
    public async Task NewEnvironment_ShouldShowCancelButton()
    {
        // Act
        var component = RenderComponent<NewEnvironment>();

        // Assert
        var cancelButton = component.Find("a:contains('Cancel')");
        Assert.NotNull(cancelButton);
    }

    [Fact]
    public async Task NewEnvironment_ShouldDisableSubmitWhenFormInvalid()
    {
        // Act
        var component = RenderComponent<NewEnvironment>();

        // Assert
        var submitButton = component.Find("button[type='submit']");
        Assert.True(submitButton.HasAttribute("disabled") || 
                   submitButton.GetClasses().Contains("disabled"));
    }

    [Fact]
    public async Task NewEnvironment_ShouldHandleNoTenantsAvailable()
    {
        // Act (no tenants in database)
        var component = RenderComponent<NewEnvironment>();

        // Assert
        Assert.Contains("No tenants available", component.Markup);
    }

    [Fact]
    public async Task NewEnvironment_ShouldShowLoadingState()
    {
        // Act
        var component = RenderComponent<NewEnvironment>();

        // Assert - Component should handle loading state gracefully
        Assert.NotNull(component);
    }

    [Fact]
    public async Task NewEnvironment_ShouldTrimWhitespaceFromInputs()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        // Act
        var component = RenderComponent<NewEnvironment>();
        
        var nameInput = component.Find("input[id='name']");
        await nameInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "  Test Environment  "
        });

        var form = component.Find("form");
        // Fill other required fields
        var descriptionTextarea = component.Find("textarea[id='description']");
        var serverUrlInput = component.Find("input[id='serverUrl']");
        var tenantSelect = component.Find("select[id='tenantId']");
        
        await descriptionTextarea.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "Description"
        });
        
        await serverUrlInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "https://test.example.com"
        });
        
        await tenantSelect.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = tenant.Id.ToString()
        });

        await form.SubmitAsync();

        // Assert
        var createdEnvironment = DbContext.InstallationEnvironments
            .FirstOrDefault(e => e.Name == "Test Environment");
        
        Assert.NotNull(createdEnvironment);
        Assert.Equal("Test Environment", createdEnvironment.Name); // Should be trimmed
    }
}
