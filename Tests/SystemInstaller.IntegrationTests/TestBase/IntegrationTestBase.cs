using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SystemInstaller.Infrastructure.Data;
using Bunit;
using Microsoft.Extensions.Logging;

namespace SystemInstaller.IntegrationTests.TestBase;

public class IntegrationTestBase : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database context
            services.RemoveAll(typeof(DbContextOptions<SystemInstallerDbContext>));
            services.RemoveAll(typeof(SystemInstallerDbContext));

            // Add in-memory database
            services.AddDbContext<SystemInstallerDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Ensure the database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SystemInstallerDbContext>();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}

public abstract class BlazorComponentTestBase : TestContext, IDisposable
{
    protected IntegrationTestBase WebApplicationFactory { get; private set; }
    protected SystemInstallerDbContext DbContext { get; private set; }

    protected BlazorComponentTestBase()
    {
        WebApplicationFactory = new IntegrationTestBase();
        
        // Register services from the web application
        var serviceProvider = WebApplicationFactory.Services;
        Services.AddSingleton(serviceProvider);
        
        // Get or create a scope and register scoped services
        var scope = serviceProvider.CreateScope();
        foreach (var service in scope.ServiceProvider.GetServices<object>())
        {
            Services.AddSingleton(service.GetType(), service);
        }

        DbContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SystemInstallerDbContext>();
        
        // Ensure clean database for each test
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
    }

    public new void Dispose()
    {
        DbContext?.Dispose();
        WebApplicationFactory?.Dispose();
        base.Dispose();
    }
}
