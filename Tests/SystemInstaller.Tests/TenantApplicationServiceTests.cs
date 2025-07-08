using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.Infrastructure.Data;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Enums;
using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Tests;

public class TenantApplicationServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly SystemInstallerDbContext _context;

    public TenantApplicationServiceTests()
    {
        // Setup in-memory database
        var services = new ServiceCollection();
        services.AddDbContext<SystemInstallerDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<SystemInstallerDbContext>();
    }

    [Fact]
    public async Task Database_ShouldBeAccessible()
    {
        // Simple test to verify database connection
        var canConnect = await _context.Database.CanConnectAsync();
        Assert.True(canConnect);
    }

    [Fact]
    public async Task TenantEntity_ShouldBeCreatable()
    {
        // Test creating a tenant entity using the proper constructor
        var email = new Email("test@example.com");
        var tenant = new Tenant("Test Tenant", email, "Test description");

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var savedTenant = await _context.Tenants.FindAsync(tenant.Id);
        Assert.NotNull(savedTenant);
        Assert.Equal("Test Tenant", savedTenant.Name);
        Assert.Equal("test@example.com", savedTenant.ContactEmail.Value);
    }

    [Fact]
    public async Task InstallationEnvironmentEntity_ShouldBeCreatable()
    {
        // Test creating an installation environment entity
        var email = new Email("test@example.com");
        var tenant = new Tenant("Test Tenant", email, "Test description");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var environment = new InstallationEnvironment(tenant.Id, "Test Environment", "Test Description");

        _context.Environments.Add(environment);
        await _context.SaveChangesAsync();

        var savedEnvironment = await _context.Environments.FindAsync(environment.Id);
        Assert.NotNull(savedEnvironment);
        Assert.Equal("Test Environment", savedEnvironment.Name);
        Assert.Equal(tenant.Id, savedEnvironment.TenantId);
    }

    [Theory]
    [InlineData(InstallationStatus.Pending)]
    [InlineData(InstallationStatus.Running)]
    [InlineData(InstallationStatus.Completed)]
    [InlineData(InstallationStatus.Failed)]
    [InlineData(InstallationStatus.Cancelled)]
    public async Task InstallationTask_WithAllStatusValues_ShouldWork(InstallationStatus status)
    {
        // Test all installation status values using InstallationTask
        var email = new Email("test@example.com");
        var tenant = new Tenant("Test Tenant", email, "Test description");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var environment = new InstallationEnvironment(tenant.Id, "Test Environment", "Test Description");
        _context.Environments.Add(environment);
        await _context.SaveChangesAsync();

        var installationTask = new InstallationTask(environment.Id, "Test Task", "Test Description")
        {
            Status = status
        };

        _context.Tasks.Add(installationTask);
        await _context.SaveChangesAsync();

        var savedTask = await _context.Tasks.FindAsync(installationTask.Id);
        Assert.NotNull(savedTask);
        Assert.Equal(status, savedTask.Status);
        Assert.Equal("Test Task", savedTask.Name);
    }

    [Fact]
    public void EmailValueObject_ShouldValidateCorrectly()
    {
        // Test email value object validation
        var validEmail = "test@example.com";
        var email = new Email(validEmail);
        
        Assert.Equal("test@example.com", email.Value);
        Assert.Equal("test@example.com", email.ToString());
        
        // Test implicit conversion
        string emailString = email;
        Assert.Equal("test@example.com", emailString);
    }

    [Fact]
    public void EmailValueObject_ShouldThrowOnInvalidEmail()
    {
        // Test email validation
        Assert.Throws<ArgumentException>(() => new Email("invalid-email"));
        Assert.Throws<ArgumentException>(() => new Email(""));
        Assert.Throws<ArgumentException>(() => new Email("   "));
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
